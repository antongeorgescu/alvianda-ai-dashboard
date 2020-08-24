"""
Routes and views for analytics endpoints.
"""

from requests import Session
from signalr import Connection
from datetime import datetime
from flask import make_response, render_template, jsonify, Response, request, url_for
from WineQuality_RestAPI import app
import time
import json
import os
import sqlite3
import sys
import uuid
import pandas as pd
import numpy as np
from sqlalchemy import create_engine
import pickle
import csv
from flask import session

from WineQuality_RestAPI.models.decisiontree_correlation_class import DecisionTreeAnalyzer
from WineQuality_RestAPI.models.data_preparation_singleton import DataPreparationSingleton
from WineQuality_RestAPI.models.algorithm_model_singleton import AlgorithmModelSingleton

from WineQuality_RestAPI.controllers.wine_analytics_controller import read_saved_dataframe_db

DB_PATH = f'{os.getcwd()}/Database/Sqlite/DatasetMLAnalytics.db'

@app.route('/api/testcontroller/trainmodel/save/tofile',methods=['GET'])
def model_test_save_tofile():
    # REF: https://www.kaggle.com/prmohanty/python-how-to-save-and-load-ml-models
    try:
        query_parameters = request.args
        sessionid = query_parameters.get('sessionid')
        response,X_test,Y_test,model,modelid = train_model(sessionid)
        
        if response == "Error":
            raise Exception(X_test)
        
        # save X_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test_{modelid}.csv'
        np.savetxt(filename, X_test[0:5], delimiter=",")

        # save Y_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test_{modelid}.csv'
        np.savetxt(filename, Y_test[0:5], delimiter=",")

        # dump model in a temporary file
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_{modelid}.pkl'
        pickle.dump(model, open(filename, 'wb'))
        
        runinfo = f'Model saved to {filename}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/testcontroller/trainmodel/load/fromfile',methods=['GET'])
def model_test_load_fromfile():
    # REF: https://www.kaggle.com/prmohanty/python-how-to-save-and-load-ml-models
    try:
        runinfo = None

        query_parameters = request.args
        modelid = query_parameters.get('modelid')
        
        # read X_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test_{modelid}.csv'
        X_test = np.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)
                
        # read Y_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test_{modelid}.csv'
        Y_test = np.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)
            
        # load the model from disk
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_{modelid}.pkl'
        loaded_model = pickle.load(open(filename, 'rb'))
        score = loaded_model.score(X_test, Y_test)

        prediction = loaded_model.predict(X_test)

        runinfo = f'Score:{score},Predictions:{prediction}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/testcontroller/trainmodel/save/todatabase',methods=['GET'])
def model_test_save_todatabase():
    # REF: https://www.kaggle.com/prmohanty/python-how-to-save-and-load-ml-models
    try:
        query_parameters = request.args
        sessionid = query_parameters.get('sessionid')
        response,X_test,Y_test,model,modelid = train_model(sessionid)
        
        if response == "Error":
            raise Exception(X_test)
        
        # save X_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test_{modelid}.csv'
        np.savetxt(filename, X_test[0:5], delimiter=",")

        # save Y_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test_{modelid}.csv'
        np.savetxt(filename, Y_test[0:5], delimiter=",")

        # dump model in a database table
        model_saved = pickle.dumps(model)
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        # check if the model_id is already serialized; if so, use UPDATE
        query = 'SELECT SerializedObject FROM SavedModel WHERE Id = ?'
        c.execute(query,(modelid,))
        row = c.fetchone()
        if row is None:
            param = (modelid,model_saved, )
            query = 'INSERT INTO SavedModel (Id,SerializedObject) VALUES (?,?)'
        else:
            param = (model_saved,modelid, )
            query = 'UPDATE SavedModel SET SerializedObject = ? WHERE Id = ?'
        c.execute(query,param)

        conn.commit()
        conn.close()

        runinfo = f'Model saved into database with key {modelid}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/testcontroller/trainmodel/load/fromdatabase',methods=['GET'])
def model_test_load_fromdatabase():
    # REF: https://www.kaggle.com/prmohanty/python-how-to-save-and-load-ml-models
    try:
        runinfo = None

        query_parameters = request.args
        modelid = query_parameters.get('modelid')
        
        # read X_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test_{modelid}.csv'
        X_test = np.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)
                
        # read Y_test[0:5]
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test_{modelid}.csv'
        Y_test = np.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)

        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        query = 'SELECT SerializedObject FROM SavedModel WHERE Id = ?'
        c.execute(query,(modelid,))
        model_saved = c.fetchone()[0]
        conn.close()

        loaded_model = pickle.loads(model_saved)
        score = loaded_model.score(X_test, Y_test)

        prediction = loaded_model.predict(X_test)

        runinfo = f'Score:{score},Predictions:{prediction}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

def train_model(sessionId):
    try:
        
        # get from database observations (X.train) and labels (y.train)
        response,x1,x2,observations, labels = read_saved_dataframe_db(sessionId) 
        if response != "Ok":
            raise Exception(response)
        
        XObservations = observations
        yLabels = labels
        
        dtanalyzer = DecisionTreeAnalyzer(XObservations,yLabels)
        dtanalyzer.scale_dataset()
        dtanalyzer.train_and_fit_model(sessionId)

        X_TEST = dtanalyzer.X_test
        Y_TEST = dtanalyzer.y_test

        accuracy = dtanalyzer.calculate_accuracy()
        cm = dtanalyzer.calculate_confusion_matrix()
        
        # temporary hold the model in AlgorithmModelSingleton
        model, model_id = dtanalyzer.get_model()

        modelalg = AlgorithmModelSingleton()
        modelalg.session_id = sessionId
        modelalg.model_id = model_id
        modelalg.description = 'temporary hold the model in AlgorithmModelSingleton class'
        modelalg.modelbinary = model

        return "OK",X_TEST[0:10], Y_TEST[0:10],model,model_id
    except Exception as error:
        return "Error",error,None,None,None
    