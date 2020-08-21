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

from WineQuality_RestAPI.models.decisiontree_correlation_class import DecisionTreeAnalyzer
from WineQuality_RestAPI.models.data_preparation_singleton import DataPreparationSingleton
from WineQuality_RestAPI.models.model_algorithm_singleton import ModelAlgorithmSingleton

DB_PATH = f'{os.getcwd()}/Database/Sqlite/DatasetMLAnalytics.db'
SAVEDFDB_PATH = f'{os.getcwd()}/Database/Sqlite/SavedDataframes.db'
mlModelAlgorithm = None

@app.route('/api/wineanalytics/validate', methods=['GET','POST'])
def validate():
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
                                 #rundata["preparred_dataset"],
                                 #rundata["field_set"],
                                 run_summary),200)

@app.route('/api/wineanalytics/runanalyzer/dataset/persist', methods=['GET','POST'])
def run_analysis_persist():
    
    try:
        description = None
        notes = None
        attributes = None
        if request.method == 'GET':
            query_parameters = request.args
            description = query_parameters.get('description')
            notes = query_parameters.get('notes')
            attributes = query_parameters.get('attributes')
        if request.method == 'POST':
            data = request.json
            description = data['description']
            notes = data['notes']
            attributes = data['attributes']

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
        
        query = 'INSERT INTO  WorkingSession (SessionId,ApplicationId,AlgorithmId,Description,Notes) '
        query += 'VALUES (?,?,?,?,?)'
        t = (guid,1,99,description,notes)
        c.execute(query,t)

        query = 'INSERT INTO  ApplicationData (SessionId,DataobjectTypeId,DataobjectName,DataobjectDescription,DataobjectValue,DataobjectAttributes) '
        query += 'VALUES (?,?,?,?,?)'
        
        #t = (guid,'processed_observations','processed_observations',observations.to_json(),attributes)
        t = (guid,1,'processed_observations','processed_observations',f'savedfdb_{guid}',attributes)
        c.execute(query,t)
        
        t = (guid,2,'processed_labels','processed_labels',labels.to_json(),'')
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
        #return jsonify(isError= False,
        #            message= "Success",
        #            statusCode= 200,
        #            data= data), 200    

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

        query = 'select ad.SessionId, a.Name, a.Description,'
        query += 'ag.Name, ag.DisplayName, t.Name,'
        query += 't.Description, ad.DataobjectName,'
        query += 'ad.DataobjectDescription, ad.DataobjectValue, ad.UpdateDt '
        query += 'from ApplicationData ad '
        query += 'inner join Application a on ad.ApplicationId = a.ApplicationId '
        query += 'inner join Algorithm ag on ag.Id = ad.AlgorithmId '
        query += 'inner join AlgorithmType t on ag.TypeId = t.Id'
        c.execute(query)
        rows = c.fetchall()

        result = json.dumps( [dict(ix) for ix in rows] )
        return make_response(jsonify(result), 200)
        #return jsonify(isError= False,
        #            message= "Success",
        #            statusCode= 200,
        #            data= data), 200    

    except (RuntimeError, TypeError, NameError) as err:
        return make_response(jsonify(err.args), 500)
    except:
        return make_response(jsonify(sys.exc_info()[0]), 500)

@app.route('/api/wineanalytics/runanalyzer/about_confusion_matrix')
def about_confusion_matrix():
    about_file_path = f'{os.getcwd()}/WineQuality_RestAPI/models/about_confusion_matrix.txt'
    f = open(about_file_path, "r")
    return make_response(f.read(),200)

@app.route('/api/wineanalytics/runanalyzer/trainmodel')
def train_model():
    try:
        query_parameters = request.args
        algorithm = query_parameters.get('algorithm')
        sessionId = query_parameters.get('sessionid')
        
        rundata = None  # should be a dictionary
        
        # get from database observations (X.train) and labels (y.train)
        response, observations_name, labels_name, observations, labels = read_saved_dataframe_db(sessionId) 
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
        
        # temporary hold the model in ModelAlgorithmSingleton
        modelalg = ModelAlgorithmSingleton()
        model, model_id = dtanalyzer.get_model()
        modelalg.set_model(model,model_id)

        rundata = accuracy +'|'+ str(cm) + f'|modelid:{model_id}'
        return make_response(jsonify(rundata),200)
    except Exception as error:
        return make_response(error,500)

@app.route('/api/wineanalytics/runanalyzer/trainmodel/save',methods=['POST'])
def save_train_model():
    try:
        runinfo = None

        sessionid = request.json['sessionid']
        modelid = request.json['modelid']
        modeldescription = request.json['modeldescription']
        
        # retrieve model from temporary holding class
        modelalg = ModelAlgorithmSingleton()
        trainmodel, trainmodel_id = modelalg.get_model()

        if modelid != trainmodel_id:
            raise Exception('ModelAlgorithSingleton class corrupted. Abort execution.')

        #TODO: will remove the file save, and leave only db save
        # dump model in a temporary file
        filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/{trainmodel_id}.sav'
        
        writefile = open(filename, 'wb')
        pickle.dump(trainmodel, writefile)
        writefile.close()

        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'INSERT INTO ApplicationData (SessionId,DataobjectTypeId,'
        query += 'DataobjectName,DataobjectDescription,DataobjectBlob) '
        query += 'VALUES (?,?,?,?,?)'

        readfile = open(filename, 'rb')
        btrainmodel = readfile.read()
        readfile.close()
        
        # Convert data into tuple format
        data_tuple = (sessionid,3,trainmodel_id, modeldescription, btrainmodel)
        cursor.execute(query, data_tuple)
        conn.commit()
        cursor.close()

        runinfo = f'Saved into database trained model {trainmodel_id} and temporary saved in file system at {filename}'
        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/wineanalytics/runanalyzer/trainmodel/load',methods=['GET'])
def load_train_model():
    try:
        runinfo = None

        modelId = request.args.get('modelid', default='', type=str)
        
        if modelId == '':
            raise Exception('Unable to read model_id parameter.Abort the execution.')

        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'SELECT SessionId,DataobjectTypeId,DataobjectName,DataobjectDescription,DataobjectBlob '
        query += 'FROM Application Data WHERE DataobjectTypeId = 3 AND DataobjectName = ?'
       
        cursor.execute(query, (modelId,))
        record = cursor.fetchone()
        
        # a Python object (dict):
        result = {
            "sessionid": record['SessionId'],
            "modelid": record['DataobjectName'],
            "modelblob": record['DataobjectBlob'],
            "modeldescription": record['DataobjectDescription']
        }

        # convert into JSON:
        runinfo = json.dumps(result)

        cursor.close()

        modelalg = ModelAlgorithmSingleton()
        modelalg.set_model(modelblob,modelId)

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()


def read_saved_dataframe_db(sessionId):
    try:
        # get from database observations (X.train) and labels (y.train)
        conn = sqlite3.connect(DB_PATH)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()
        query = 'SELECT DataobjectName, DataobjectAttributes, DataobjectValue FROM ApplicationData '
        query += f'WHERE SessionId = \'{sessionId}\''
        c.execute(query)
        rows = c.fetchall()
        conn.close()

        observations_name = rows[0]['DataobjectName']
        labels_name = rows[1]['DataobjectName']
        
        jdata = json.loads(rows[1]["DataobjectValue"])
        labels = np.array(list(jdata.values()), dtype=int)
                
        conn2 = sqlite3.connect(SAVEDFDB_PATH)
        query = f'SELECT * FROM \'{sessionId}\''
        observations = pd.read_sql_query(query, conn2)
        conn2.close()      

        # Verify that result of SQL query is stored in the dataframe
        print(observations.head())

        return "Ok",observations_name,labels_name,observations,labels
    
    except Exception as error:
        return error

@app.route('/api/wineanalytics/runanalyzer/persist/dataframe')
def get_saved_dataframe_db():
    try:
        query_parameters = request.args
        sessionId = query_parameters.get('sessionid')
        
        response, observations_name, labels_name, observations, labels = read_saved_dataframe_db(sessionId)     

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