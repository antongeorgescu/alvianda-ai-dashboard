import uuid
import sqlite3

class AlgorithmModelSingleton:
    class __AlgorithmModelSingleton:
        def __init__(self):
            self.model = None
            self.model_id = None
            self.modeldescription = None
            self.db_path = None
            self.sessionid = None
            return
        def __set_db_path__(self,dbpath):
            self.db_path = dbpath
        def __get_db_path__(self):
            return self.db_path
        def __set_session_id__(self,sessionid):
            self.sessionid = sessionid
        def __get_session_id__(self):
            return self.sessionid
        def __set_model_id__(self,modelid):
            self.model_id = modelid
        def __get_model_id__(self):
            return self.model_id
        def __set_model_description__(self,description):
            self.modeldescription = description
        def __get_model_description__(self):
            return self.modeldescription
        
        def __set_model__(self,model):
            if self.model_id is None:
                raise Exception('Model_id not provided. Unable to complete.')
            self.model = model
        def __get_model__(self):
            # return tuple (modelid, modelblob)
            if self.model is None:
                # retrieve model blob data from db
                if self.db_path is None:
                    raise Exception ('Model is not in memory and no database path provided.Abort execution.')
                else:
                    try:
                        # get the model in binary format from sqlite BLOB record
                        conn = sqlite3.connect(self.db_path)
                        cursor = conn.cursor()
                        
                        query = 'SELECT ad.DataobjectName,ad.DataobjectDescription,ad.DataobjectBlob ' 
                        query += 'FROM ApplicationData ad '
                        query += 'WHERE ad.DataobjectTypeId = 3 AND ad.DataobjectName = ?'
                        
                        cursor.execute(query, (self.model_id,))
                        records = cursor.fetchall()
                        conn.close()

                        self.model_id = records[0][0]
                        self.modeldescription = records[0][1]
                        self.model = records[0][2]

                        return self.model
                    except Exception as error:
                        if (conn):
                            conn.rollback()
                        raise Exception(error)
                    finally:
                        if (conn):
                            conn.close()
            else:
                return self.model 
        
        def __save_model_to_db__(self):
            # save model in db with model_id
            # use model_id, model_nm
            # save the model in binary format to sqlite BLOB record
            
            try:
                if self.db_path is None:
                    raise Exception('Database path is missing. Abort execution.')
                if self.sessionid is None:
                    raise Exception('Session id is missing. Abort execution.')
                if self.model_id is None:
                    raise Exception('Model id is missing. Abort execution.')
                
                conn = sqlite3.connect(self.db_path)
                cursor = conn.cursor()
                
                query = 'INSERT INTO ApplicationData (SessionId,DataobjectTypeId,'
                query += 'DataobjectName,DataobjectDescription,DataobjectBlob) '
                query += 'VALUES (?,3,?,?,?)'

                # Convert data into tuple format
                data_tuple = (self.sessionid,self.model_id, self.modeldescription, self.model,)
                cursor.execute(query, data_tuple)
                conn.commit()
                cursor.close()
            except Exception as error:
                if not conn is None:
                    conn.rollback()
                raise Exception(error)
            finally:
                if not conn is None:
                    conn.close()
    instance = None
    def __init__(self):
        if not AlgorithmModelSingleton.instance:
            AlgorithmModelSingleton.instance = AlgorithmModelSingleton.__AlgorithmModelSingleton()
    
    def session_id_setter(self,sessionid):
        return self.instance.__set_session_id__(sessionid)
    def session_id_getter(self):
        return self.instance.__get_session_id__()
    session_id = property(session_id_getter,session_id_setter)

    
    def model_id_setter(self,modelid):
        return self.instance.__set_model_id__(modelid)
    def model_id_getter(self):
        return self.instance.__get_model_id__()
    model_id = property(model_id_getter,model_id_setter)

    def model_description_setter(self,description):
        return self.instance.__set_model_description__(description)
    def model_description_getter(self):
        return self.instance.__get_model_description__()
    description = property(model_description_getter,model_description_setter)
    
    def modelbinary_setter(self,modelblob):
        return self.instance.__set_model__(modelblob)
    def modelbinary_getter(self):
        return self.instance.__get_model__()
    modelbinary = property(modelbinary_getter,modelbinary_setter)
    
    def dbpath_setter(self,db_path):
        return self.instance.__set_db_path__(db_path)
    def dbpath_getter(self):
        return self.instance.__get_db_path__()
    dbpath = property(dbpath_getter,dbpath_setter)

    def save_model_to_db(self):
        return self.instance.__save_model_to_db__()

    
    