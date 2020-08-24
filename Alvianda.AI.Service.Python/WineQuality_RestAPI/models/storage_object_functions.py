import pickle
import sqlite3
import json
import pandas as pd, numpy as np

def model_save_todatabase(sessionid,modelid,description,model,dbpath):
    try:
        
        # dump model in a database table
        model_saved = pickle.dumps(model)
        conn = sqlite3.connect(dbpath)
        #conn.row_factory = sqlite3.Row
        c = conn.cursor()

        # check if the model_id is already serialized; if so, use UPDATE
        query = 'SELECT COUNT(*) FROM ApplicationData WHERE DataobjectName = ? AND SessionId = ?'
        c.execute(query,(modelid,sessionid,))
        rowno = c.fetchone()[0]
        if rowno == 0:
            param = (sessionid,modelid,description,model_saved, )
            query = 'INSERT INTO ApplicationData (SessionId,DataobjectTypeId,'
            query += 'DataobjectName,DataobjectDescription,DataobjectBlob) '
            query += 'VALUES (?,3,?,?,?)'
        else:
            param = (description,model_saved,sessionid,modelid, )
            query = 'UPDATE ApplicationData SET DataobjectDescription = ?, DataobjectBlob = ? '
            query += 'WHERE SessionId = ? AND DataobjectName = ?'
        c.execute(query,param)

        conn.commit()
        conn.close()

        return
    except Exception as error:
        raise Exception(error)

def model_load_fromdatabase(sessionid,modelid,dbpath):
    try:
        
        conn = sqlite3.connect(dbpath)
        c = conn.cursor()

        query = 'SELECT DataobjectBlob,DataobjectDescription,DataobjectAttributes FROM ApplicationData '
        query += ' WHERE SessionId = ? AND DataobjectName = ?'
        c.execute(query,(sessionid,modelid,))
        result = c.fetchone()
        model_saved = result[0]
        description = result[1]
        attributes = result[2]
        conn.close()

        loaded_model = pickle.loads(model_saved)
        
        return loaded_model,description,attributes
    except Exception as error:
        raise Exception(error)

def model_update_description(sessionid,modelid,description,dbpath):
    try:
        
        conn = sqlite3.connect(dbpath)
        #conn.row_factory = sqlite3.Row
        c = conn.cursor()

        # check if the model_id is already serialized; if so, use UPDATE
        query = 'SELECT COUNT(*) FROM ApplicationData WHERE DataobjectName = ? AND SessionId = ?'
        c.execute(query,(modelid,sessionid,))
        rowno = c.fetchone()[0]
        if rowno == 0:
            raise Exception(f'No model with id={modelid} created. Abort execution.')
        else:
            param = (description,sessionid,modelid, )
            query = 'UPDATE ApplicationData SET DataobjectDescription = ? '
            query += 'WHERE SessionId = ? AND DataobjectName = ?'
        c.execute(query,param)

        conn.commit()
        conn.close()

        return
    except Exception as error:
        raise Exception(error)

def read_saved_dataframe(sessionId,app_dbpath,df_dbpath):
    try:
        # get from database observations (X.train) and labels (y.train)
        conn = sqlite3.connect(app_dbpath)
        conn.row_factory = sqlite3.Row
        c = conn.cursor()
        query = 'SELECT DataobjectName, DataobjectAttributes, DataobjectText FROM ApplicationData '
        query += f'WHERE SessionId = ?'
        c.execute(query,(sessionId,))
        rows = c.fetchall()
        conn.close()

        observations_name = rows[0]['DataobjectName']
        labels_name = rows[1]['DataobjectName']
        
        jdata = json.loads(rows[1]["DataobjectText"])
        labels = np.array(list(jdata.values()), dtype=int)
                
        conn2 = sqlite3.connect(df_dbpath)
        query = f'SELECT * FROM \'{sessionId}\''
        observations = pd.read_sql_query(query, conn2)
        conn2.close()      

        # Verify that result of SQL query is stored in the dataframe
        print(observations.head())

        return "Ok",observations_name,labels_name,observations,labels
    
    except Exception as error:
        return error