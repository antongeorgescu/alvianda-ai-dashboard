import uuid

class ModelAlgorithmSingleton:
    class __ModelAlgorithmSingleton:
        def __init__(self):
            return
        def __set_session__(self,sessionid):
            self.sessionid = sessionid
        def __set_model__(self,model,model_id):
            self.model = model
            self.model_id = model_id
            return self.model_id
        def __get_model__(self):
            return self.model, self.model_id
        def __save_model__(self):
            # save model in db with model_id
            # use model_id, model_nm
            return
    instance = None
    def __init__(self):
        if not ModelAlgorithmSingleton.instance:
            ModelAlgorithmSingleton.instance = ModelAlgorithmSingleton.__ModelAlgorithmSingleton()
    def set_model(self,model,model_name):
        return self.instance.__set_model__(model,model_name)
    def get_model(self):
        return self.instance.__get_model__()
    def save_model(self):
        return self.instance.__save_model__()
    