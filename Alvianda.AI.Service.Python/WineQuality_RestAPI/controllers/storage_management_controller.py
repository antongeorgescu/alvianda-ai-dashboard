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

DB_PATH = f'{os.getcwd()}/Database/Sqlite/DatasetMLAnalytics.db'
SAVEDFDB_PATH = f'{os.getcwd()}/Database/Sqlite/SavedDataframes.db'

@app.route('/api/storagemanagement/validate', methods=['GET','POST'])
def validate_storage_management_controller():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    if request.method == 'GET':
        d = f'validate storage management controller method=GET at {current_time}'
        return make_response(jsonify(d), 200)
    
    if request.method == 'POST':
        d = request.json
        return make_response(jsonify(d), 200)

@app.route('/api/storagemanagement/deletemodels/allfiles', methods=['GET','POST'])
def delete_models_all_files():
    runinfo = "Not implemented"
    return make_response(runinfo,200)


@app.route('/api/storagemanagement/deletemodels/onefile', methods=['GET','POST'])
def delete_models_one_file():
    runinfo = "Not implemented"
    return make_response(runinfo,200)

@app.route('/api/storagemanagement/test/initializedb', methods=['GET','POST'])
def test_initialize_db():
    runinfo = None

    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'INSERT INTO ApplicationData (SessionId,DataobjectTypeId,DataobjectName,DataobjectDescription,DataobjectText) '
        query += 'VALUES (?,?,?,?,?)'

        params = ('319b44d5-71b6-47e3-bcd3-a94ba27a0f2b',
                    1,
                    'processed_observations',
                    'processed_observations',
                    'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce scelerisque quam nibh, vitae lobortis turpis facilisis mollis. Maecenas mollis ac lacus nec pellentesque',
                )
        
        cursor.execute(query,params)
        inserts1 = cursor.rowcount
        conn.commit()

        # convert into JSON:
        runinfo = json.dumps(f'Successful db initialization. #Rows inserted into ApplicationData table:{inserts1}')

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/storagemanagement/deletemodels/alldbrecs', methods=['GET','POST'])
def delete_models_all_dbrecs():
    runinfo = None

    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'DELETE FROM ApplicationData'
        
        cursor.execute(query)
        rowsdeleted = cursor.rowcount
        conn.commit()

        # convert into JSON:
        runinfo = json.dumps(f'# rows deleted:{rowsdeleted}')

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()


@app.route('/api/storagemanagement/deletemodels/onedbrec', methods=['GET','POST'])
def delete_models_one_dbrec():
    runinfo = None
    modelId = request.args.get('modelid', default='', type=str)
    sessionId = request.args.get('sessionid', default='', type=str)

    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'DELETE FROM ApplicationData WHERE SessionId = ? AND DataobjectName = ?'
        
        cursor.execute(query,(sessionId,modelId,))
        rowsdeleted = cursor.rowcount

        conn.commit()

        # convert into JSON:
        runinfo = json.dumps(f'# rows deleted:{rowsdeleted}')

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/storagemanagement/worksession/all', methods=['GET','POST'])
def get_all_db_entities():
    runinfo = None
    
    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'SELECT ws.SessionId, a.Name, alg.DisplayName, ws.Description, ws.Notes, ws.CreatedOn '
        query += 'FROM WorkingSession ws ' 
        query += 'INNER JOIN Application a ON ws.ApplicationId = a.Id '
        query += 'INNER JOIN Algorithm alg ON alg.Id = ws.AlgorithmId '
        query += 'INNER JOIN AlgorithmType aty ON aty.Id = alg.TypeId'
        
        cursor.execute(query)
        rows = cursor.fetchall()

        result = []
        for row in rows:
            result.append(
                {
                    "SessionId" : row[0],
                    "ApplicationName" : row[1],
                    "AlgorithmName" : row[2],
                    "SessionDescription" : row[3],
                    "SessionNotes" : row[4],
                    "SessionCreatedOn" : row[5]
                }
            )
        
        # convert into JSON:
        runinfo = json.dumps(result)

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/storagemanagement/applicationdata/all', methods=['GET','POST'])
def get_all_db_appdata():
    runinfo = None
        
    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'SELECT DISTINCT ws.SessionId, a.Name, alg.DisplayName, ws.Description, ws.Notes, ws.CreatedOn '
        query += 'FROM WorkSession ws ' 
        query += 'INNER JOIN Application a ON ws.ApplicationId = a.Id '
        query += 'INNER JOIN Algorithm alg ON alg.Id = ws.AlgorithmId '
        query += 'INNER JOIN AlgorithmType aty ON aty.Id = alg.TypeId'
        
        cursor.execute(query)
        rows = cursor.fetchall()

        result = []
        for row in rows:
            result.append(
                {
                    "SessionId" : row[0],
                    "ApplicationName" : row[1],
                    "AlgorithmName" : row[2],
                    "SessionDescription" : row[3],
                    "SessionNotes" : row[4],
                    "SessionCreatedOn" : row[5]
                }
            )
        
        # convert into JSON:
        runinfo = json.dumps(result)

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()

@app.route('/api/storagemanagement/applicationdata', methods=['GET','POST'])
def get_by_session_db_appdata():
    runinfo = None
    sessionId = request.args.get('sessionid', default='', type=str)    
    try:
        # save the model in binary format to sqlite BLOB record
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        query = 'SELECT DISTINCT ad.DataobjectName, ad.DataobjectDescription,doty.Name,doty.Description '
        query += 'FROM ApplicationData ad ' 
        query += 'INNER JOIN DataobjectType doty ON doty.Id = ad.DataobjectTypeId '
        query += 'WHERE ad.SessionId = ?'

        cursor.execute(query,(sessionId,))
        rows = cursor.fetchall()

        result = []
        for row in rows:
            result.append(
                {
                    "DataobjectName" : row[0],
                    "DataobjectDescription" : row[1],
                    "DataobjectTypeName" : row[2],
                    "DataobjectTypeDescription" : row[3],
                }
            )
        
        # convert into JSON:
        runinfo = json.dumps(result)

        cursor.close()

        return make_response(runinfo,200)
    except Exception as error:
        return make_response(error,500)
    finally:
        if (conn):
            conn.close()


