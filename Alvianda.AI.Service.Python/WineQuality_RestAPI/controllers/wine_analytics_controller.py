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
import hashlib

from WineQuality_RestAPI.models.decisiontree_correlation_class import DecisionTreeAnalyzer
from WineQuality_RestAPI.models.data_preparation_singleton import DataPreparationSingleton
from WineQuality_RestAPI.models.storage_object_functions import *

DB_PATH = f'{os.getcwd()}/Database/Sqlite/DatasetMLAnalytics.db'
SAVEDFDB_PATH = f'{os.getcwd()}/Database/Sqlite/SavedDataframes.db'
mlModelAlgorithm = None

@app.route('/api/wineanalytics/validate', methods=['GET','POST'])
def validate_wineanalytics_controller():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    if request.method == 'GET':
        d = f'validate analytics controller method=GET at {current_time}'
        return make_response(jsonify(d), 200)
    
    if request.method == 'POST':
        d = request.json
        return make_response(jsonify(d), 200)

@app.route('/api/wineanalytics/algorithms')
def algorithmlist():
    query_parameters = request.args
    algorithmType = query_parameters.get('type')

    try:
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        t = (int(algorithmType),)
        c.execute('SELECT Id,Name,DisplayName,Description FROM Algorithm WHERE TypeId=?', t)
        rows = c.fetchall()

        #return make_response(jsonify(rows), 200)
        result = json.dumps( [dict(ix) for ix in rows] )
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/worksessions')
def workingsessionslist():
    try:
        query_parameters = request.args
        applicationId = query_parameters.get('applicationid')

        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        t = (int(applicationId),)
        c.execute('SELECT SessionId, Description, CreatedOn from WorkingSession WHERE ApplicationId=?', t)
        rows = c.fetchall()
        
        #return make_response(jsonify(rows), 200)
        result = json.dumps( [dict(ix) for ix in rows] )
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/worksessions/trainmodel')
def trainmodelsessionslist():
    try:
        query_parameters = request.args
        applicationId = query_parameters.get('applicationid')

        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        query = 'SELECT ws.SessionId,a.DisplayName,ws.Description, ws.Notes, ws.CreatedOn from WorkingSession ws '
        query += 'INNER JOIN ApplicationData ad ON ad.SessionId = ws.SessionId ' 
        query += 'INNER JOIN Algorithm a ON a.Id = ws.AlgorithmId '
        query += 'WHERE ad.DataobjectName = \'processed_observations\' '
        query += 'AND a.Id < 99 AND ws.ApplicationId = ?'

        t = (int(applicationId),)
        c.execute(query, t)
        rows = c.fetchall()
        
        #return make_response(jsonify(rows), 200)
        result = json.dumps( [dict(ix) for ix in rows] )
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/worksessions/details')
def workingsessiondetails():
    try:
        query_parameters = request.args
        sessionId = query_parameters.get('sessionid')

        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        t = (sessionId,)
        c.execute('SELECT Description, Notes, CreatedOn from WorkingSession WHERE SessionId=?', t)
        row = c.fetchone()
        
        result = json.dumps(dict(row))
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/worksessions/trainmodel/details')
def trainsessiondetails():
    try:
        query_parameters = request.args
        sessionId = query_parameters.get('sessionid')

        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        query = 'SELECT ad.DataobjectAttributes,a.DisplayName,ws.Description, ws.Notes, ws.CreatedOn from WorkingSession ws '
        query += 'INNER JOIN ApplicationData ad ON ad.SessionId = ws.SessionId '
        query += 'INNER JOIN Algorithm a ON a.Id = ws.AlgorithmId '
        query += 'WHERE ad.DataobjectName = ? '
        query += 'AND ws.SessionId=? '
        query += 'AND a.Id < 99'

        t = ('processed_observations',sessionId,)
        c.execute(query, t)
        row = c.fetchone()
        
        result = json.dumps(dict(row))
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/dobjs')
def saveddataobjectlist():
    query_parameters = request.args
    applicationId = query_parameters.get('applicationid')

    try:
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        t = (int(applicationId),)
        c.execute('SELECT SessionId,DataobjectName,DataobjectDescription,DataobjectValue FROM ApplicationData WHERE ApplicationId=?', t)
        rows = c.fetchall()
        for row in rows:
            print(row)
        observations = pd.DataFrame(rows[0])
        labels = np.Array(rows[1])

        dtanalyzer = DataPreparationSingleton()
        # save first row in 'observations'
        # save second row in 'labels'
        dtanalyzer.set_observations_and_labels(observations, labels)

        #return make_response(jsonify(rows), 200)
        result = json.dumps( [dict(ix) for ix in rows] )
        return result
    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/runanalyzer/dataset/prepare')
def run_analysis():
    #query_parameters = request.args
    #algorithm = query_parameters.get('algorithm')
    
    REDWINE_PATH = f'{os.getcwd()}/WineQuality_RestAPI/datasets/winequality-red.csv'
    WHITEWINE_PATH = f'{os.getcwd()}/WineQuality_RestAPI/datasets/winequality-white.csv'

    rundata = {
        "startproc" : "",
        "endproc" : "",
        "duration" : 0.0,
        "cshistogramcharts" : "",
        "cshistogramtitles" : "",
        "quality_drop" : "",
        "correlationchart" : "",
        "correlationtitle" : "",
        "correlated_attributes": ""
    }
    dtanalyzer = None

    try:
        startproc = time.time()
        startprocstr = datetime.now().strftime("%H:%M:%S.%f")       
            
        dtanalyzer = DataPreparationSingleton(REDWINE_PATH,WHITEWINE_PATH)  
        rundata["attributes"] = dtanalyzer.get_attributes()
        rundata["cshistogramcharts"],rundata["cshistogramtitles"] = dtanalyzer.create_histrograms()

        rundata["quality_drop"] = dtanalyzer.reduce_dimensionality()

        rundata["correlated_attributes"],rundata["correlationchart"],rundata["correlationtitle"] = dtanalyzer.identify_correlations()

        #rundata["preparred_dataset"],rundata["fieldSet"] = dtanalyzer.get_observations_and_labels()

        endprocstr = datetime.now().strftime("%H:%M:%S.%f")
        proc_duration = time.time() - startproc
        duration = "{:.2f}".format(proc_duration)
    except Exception as error:
        return make_response(error,500)
    run_summary = f'run dataset analyzer from {startprocstr} to {endprocstr}, for the duration of {duration} sec'
    
    return make_response(jsonify(rundata["cshistogramcharts"],
                                 rundata["cshistogramtitles"],
                                 rundata["quality_drop"],
                                 rundata["correlationchart"],
                                 rundata["correlationtitle"],
                                 rundata["correlated_attributes"],
                                 rundata["attributes"],
                                 #rundata["preparred_dataset"],
                                 #rundata["field_set"],
                                 run_summary),200)

@app.route('/api/wineanalytics/runanalyzer/dataset/persist', methods=['GET','POST'])
def run_analysis_persist():
    
    try:
        description = None
        notes = None
        attributes = None
        forcecreate = None
        if request.method == 'GET':
            query_parameters = request.args
            description = query_parameters.get('description')
            notes = query_parameters.get('notes')
            attributes = query_parameters.get('attributes')
            if (query_parameters.get('forcecreate') == True):
                forcecreate = True
            else:
                forcecreate = False
        if request.method == 'POST':
            data = request.json
            description = data['description']
            notes = data['notes']
            attributes = data['attributes']
            if (data['forcecreate'] == True):
                forcecreate = True
            else:
                forcecreate = False
            
        if (description == None):
            description = '[System] Saved preparred data objects'
        if (notes == None):
            notes = '[System] No notes provided by user'
        if (attributes == None):
            attributes = '[System] No persisted data object attributes available'

        startproc = time.time()
        startprocstr = datetime.now().strftime("%H:%M:%S.%f")       
        
        dtanalyzer = DataPreparationSingleton()
        observations, labels = dtanalyzer.get_observations_and_labels() 
            
        # save in db both observations and labels 
        guid = str(uuid.uuid4())
        conn = sqlite3.connect(DB_PATH)

        c = conn.cursor()

        hash_in_dataset = hashlib.md5(observations.to_json().encode()).hexdigest()

        if not forcecreate:
            # check if there's already saved an identical DataobjectText (by using Hashvalue)
            query = 'SELECT SessionId, DataobjectText, Hashvalue FROM ApplicationData'
            c.execute(query)
            rows = c.fetchall()
            for row in rows:
                hash_saved_dataset = row[2]
                if hash_saved_dataset == hash_in_dataset:
                    sessionId = row[0]
                    query = 'SELECT SessionId, DataobjectText, Hashvalue FROM ApplicationData'
                    run_summary = f'Dataset of observations and/or labels already saved in session_id:{sessionId}.Abort current execution.'
                    return make_response(jsonify(run_summary),200)
        
        query = 'INSERT INTO  WorkingSession (SessionId,ApplicationId,AlgorithmId,Description,Notes) '
        query += 'VALUES (?,?,?,?,?)'
        t = (guid,1,99,description,f'[{datetime.now()}] {notes}')
        c.execute(query,t)

        query = 'INSERT INTO  ApplicationData (SessionId,DataobjectTypeId,DataobjectName,DataobjectDescription,DataobjectText,DataobjectBlob,DataobjectAttributes,Hashvalue) '
        query += 'VALUES (?,?,?,?,?,?,?,?)'
        
        t = (guid,1,'processed_observations','processed_observations',observations.to_json(),f'savedfdb_{guid}',attributes,str(hash_in_dataset))
        c.execute(query,t)
        
        t = (guid,2,'processed_labels','processed_labels',labels.to_json(),None,None,str(hashlib.md5(labels.to_json().encode()).hexdigest()))
        c.execute(query,t)
        conn.commit()

        #save dataframe as separate table with sessionid name
        engine = create_engine(f'sqlite:///{SAVEDFDB_PATH}', echo=False)
        sqlite_connection = engine.connect()
        sqlite_table = guid
        observations.to_sql(sqlite_table, sqlite_connection, if_exists='fail')
        sqlite_connection.close()
        
        endprocstr = datetime.now().strftime("%H:%M:%S.%f")
        proc_duration = time.time() - startproc
        duration = "{:.2f}".format(proc_duration)
        run_summary = f'[sessionid:{guid}] run dataset and labels persisting procedure from {startprocstr} to {endprocstr}, for the duration of {duration} sec'
        return make_response(jsonify(run_summary),200)
    except sqlite3.Error as error:
        if (conn):
            conn.rollback()
        return make_response(f'Failed to insert data into sqlite table: {error}',500)
    except Exception as error:
        if (conn):
            conn.rollback()
        if (sqlite_connection):
            sqlite_connection.rollback();    
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()
        

@app.route('/api/wineanalytics/processdata', methods=['GET', 'POST','DELETE'])
def processdata():
    # FIXME: Pass the right parameters to endpoint
    try:
        if request.method == 'GET':
            sessionId = request.args.get('sessionid', default='', type=str)
            applicationId = request.args.get('applicationid', default='', type=str)
            algorithmid = request.args.get('algorithmid', default=0, type=int)
        
            conn = sqlite3.connect(DB_PATH)
            conn.row_factory = sqlite3.Row
            c = conn.cursor()

            t = (1,)
            c.execute('SELECT Id,Name,DisplayName,Description FROM Algorithm WHERE TypeId=?', t)
            rows = c.fetchall()

            #return make_response(jsonify(rows), 200)
            result = json.dumps( [dict(ix) for ix in rows] )

        if request.method == 'POST':
            sessionid = request.form.get('sessionid')
            algorithmid = request.form.get('algorithmid')
            applicationid = request.form.get('applicationid')
            doname = request.form.get('dataobjectname')
            dodescription = request.form.get('dataobjectdescription')
            dovalue = request.form.get('dataobjectvalue')
            
            ## Insert the information in the database
            conn = sqlite3.connect(DB_PATH)
            c = comm.cursor()
            query = 'INSERT INTO  ApplicationData (SessionId,ApplicationId,AlgorithmId) '
            query += f'VALUES (\'{sessionid}\',{algorithmid},{applicationid},\'{doname}\',\'{dodescription}\',\'{dovalue}\')'
            c.execute(query)
            ##Do something like insert in DB or Render somewhere etc. it's up to you....... :)
        
        return make_response(jsonify(result), 200)

    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/processdata/all', methods=['GET'])
def processdataall():
    try:
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        query = 'SELECT ad.SessionId, a.Name, a.Description,'
        query += 'ag.Name, ag.DisplayName, t.Name,'
        query += 't.Description, ad.DataobjectName,'
        query += 'ad.DataobjectDescription, ad.DataobjectValue, ad.UpdateDt '
        query += 'FROM ApplicationData ad '
        query += 'INNER JOIN Application a ON ad.ApplicationId = a.ApplicationId '
        query += 'INNER JOIN Algorithm ag ON ag.Id = ad.AlgorithmId '
        query += 'INNER JOIN AlgorithmType t ON ag.TypeId = t.Id'
        c.execute(query)
        rows = c.fetchall()

        result = json.dumps( [dict(ix) for ix in rows] )
        return make_response(jsonify(result), 200)

    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/runanalyzer/about_confusion_matrix')
def about_confusion_matrix():
    about_file_path = f'{os.getcwd()}/WineQuality_RestAPI/models/about_confusion_matrix.txt'
    f = open(about_file_path, "r")
    return make_response(f.read(),200)

@app.route('/api/wineanalytics/runanalyzer/trainmodel', methods=['POST'])
def train_model():
    try:
        algorithm = request.json['algorithm']
        sessionId = request.json['sessionid']
        description = request.json['description']
        notes = request.json['notes']

        model = None
        modelId = None
        
        rundata = None  # should be a dictionary
        
        # get from database observations (X.train) and labels (y.train)
        response, observations_name, labels_name, observations, labels = read_saved_dataframe(sessionId,DB_PATH,SAVEDFDB_PATH) 
        if response != "Ok":
            raise Exception(response)
        
        XObservations = observations
        yLabels = labels
        
        if (algorithm == "decision-tree"):
            dtanalyzer = DecisionTreeAnalyzer(XObservations,yLabels)
            dtanalyzer.scale_dataset()
            dtanalyzer.train_and_fit_model(sessionId)
            accuracy = dtanalyzer.calculate_accuracy()
            cm = dtanalyzer.calculate_confusion_matrix()
            
            model, modelId = dtanalyzer.get_model()
        
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()

        query = 'SELECT Id FROM Algorithm WHERE Name = ?'
        c.execute(query,(algorithm,))
        algorithmId = c.fetchone()[0]

        param = (sessionId, algorithmId, description,notes,)
        query = 'INSERT INTO WorkingSession (SessionId,ApplicationId,AlgorithmId,Description,Notes) '
        query += 'VALUES (?,1,?,?,?)'
        c.execute(query,param)
        conn.commit()

        # save model into database
        model_save_todatabase(sessionId,modelId,description,model,DB_PATH)

        rundata = accuracy +'|'+ str(cm) + f'|modelid:{modelId}'
        return make_response(jsonify(rundata),200)
    except Exception as error:
        if (conn):
            conn.rollback()
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/wineanalytics/runanalyzer/trainmodel/update',methods=['POST'])
def save_train_model():
    try:
        runinfo = None

        sessionid = request.json['sessionid']
        modelid = request.json['modelid']
        description = request.json['modeldescription']
               
        # update description of model
        model_update_description(sessionid,modelid,description,DB_PATH)

        runinfo = f'Updated description for trained model {modelid}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/wineanalytics/runanalyzer/trainmodel/load',methods=['GET'])
def load_train_model():
    try:
        modelId = request.args.get('modelid', default='', type=str)
        sessionId = request.args.get('sessionid', default='', type=str)
        
        if modelId == '':
            raise Exception('Unable to read model_id parameter.Abort the execution.')

        loaded_model,description,attributes = model_load_fromdatabase(sessionId,modelId,DB_PATH)
        
        modelinfo = {
            "model_id" : modelId,
            "session_id" : sessionId,
            "description" : description
        }
        
        # convert into JSON:
        runinfo = json.dumps(modelinfo)

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/wineanalytics/runanalyzer/trainmodel/predict',methods=['POST'])
def model_predict():
    # REF: https://www.kaggle.com/prmohanty/python-how-to-save-and-load-ml-models
    try:
        runinfo = None

        modelId = request.json['modelid']
        sessionId = request.json['sessionid']
        observations = request.json['observations']
        fields = request.json["attributes"]
        
        # retrieve model from storage area
        loaded_model,description,attributes = model_load_fromdatabase(sessionId,modelId,DB_PATH)
        
        #TODO: Finish this part of the code
        lstobs =  list(map(float,observations.split(',')))
        dfobs = pd.DataFrame([lstobs])
        print(dfobs)
        
        prediction = loaded_model.predict(dfobs)
        
        runinfo = f'Predicted wine quality for {modelId}:{prediction}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/wineanalytics/runanalyzer/trainmodel/saved/listall',methods=['GET'])
def list_all_models():
    try:
        runinfo = None

        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'SELECT SessionId,DataobjectName,DataobjectDescription,DataobjectBlob '
        query += 'FROM ApplicationData WHERE DataobjectTypeId = 3'
       
        cursor.execute(query)
        records = cursor.fetchall()
        
        results = []
        for record in records:
            # a Python object (dict):
            result = {
                "sessionid": record[0],
                "modelid": record[1],
                "modeldescription": record[2]
            }
            results.append(result)

        # convert into JSON:
        runinfo = json.dumps(results)

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/wineanalytics/runanalyzer/trainmodel/saved/listone',methods=['GET'])
def list_one_model():
    try:
        runinfo = None

        modelId = request.args.get('modelid', default='', type=str)
        sessionId = request.args.get('sessionid', default='', type=str)
        
        if modelId == '':
            raise Exception('Unable to read model_id parameter.Abort the execution.')

        # retrieve model from storage area
        loaded_model,description,attributes = model_load_fromdatabase(sessionId,modelId,DB_PATH)
        
        result = {
            "modelid" : modelId,
            "sessionid" : sessionId,
            "description" : description
        }
        
        # convert into JSON:
        runinfo = json.dumps(result)

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)



@app.route('/api/wineanalytics/runanalyzer/persist/dataframe')
def get_saved_dataframe_db():
    try:
        query_parameters = request.args
        sessionId = query_parameters.get('sessionid')
        
        response, observations_name, labels_name, observations, labels = read_saved_dataframe(sessionId,DB_PATH,SAVEDFDB_PATH)     

        if response != "Ok":
            raise Exception(response)

        # Verify that result of SQL query is stored in the dataframe
        print(observations.head())

        run_summary = f'[sessionid:{sessionId}] retrieved observations [{observations_name}] and labels [{labels_name}]'
        return make_response(run_summary,200)

    except Exception as error:
        return make_response(error,500)

@app.route('/api/wineanalytics/chathub')
def chat():
    with Session() as session:
        #create a connection
        connection = Connection("http://localhost:33644/wineanalytics", session)

        #get chat hub
        chat = connection.register_hub('chat')

        #start a connection
        connection.start()

        #create new chat message handler
        def print_received_message(data):
            print('received: ', data)

        #create new chat topic handler
        def print_topic(topic, user):
            print('topic: ', topic, user)

        #create error handler
        def print_error(error):
            print('error: ', error)

        #receive new chat messages from the hub
        chat.client.on('newMessageReceived', print_received_message)

        #change chat topic
        chat.client.on('topicChanged', print_topic)

        #process errors
        connection.error += print_error

        #start connection, optionally can be connection.start()
        with connection:

            #post new message
            chat.server.invoke('send', 'Python is here')

            #change chat topic
            chat.server.invoke('setTopic', 'Welcome python!')

            #invoke server method that throws error
            chat.server.invoke('requestError')

            #post another message
            chat.server.invoke('send', 'Bye-bye!')

            #wait a second before exit
            connection.wait(1)