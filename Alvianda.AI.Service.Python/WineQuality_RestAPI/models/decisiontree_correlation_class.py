import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.preprocessing import MinMaxScaler
import time
from sklearn.metrics import accuracy_score
import sys, os
from sklearn.metrics import confusion_matrix,f1_score
from sklearn.preprocessing import StandardScaler

from WineQuality_RestAPI.models.base_algorithm_class import BaseAlgorithmClass

class DecisionTreeAnalyzer(BaseAlgorithmClass):
    def __init__(self,merged_dataset,labels):
        self.merged_data = merged_dataset
        self.y = labels
        #super()._init__(merged_dataset,field_list)
        super().__init__(self.merged_data)
    
    def scale_dataset(self,scaler = 'MINMAXSCALER'):
        # two types of scalers: MINMAXSCALER, STANDARDSCALER
        self.X = super().scale_dataset(scaler)
    
    def train_and_fit_model(self):
        # test_size: what proportion of original data is used for test set
        
        try:
            self.X_train, self.X_test, self.y_train, self.y_test = train_test_split(
                self.X, self.y, test_size=1/7.0, random_state=122)

            dtree = DecisionTreeClassifier(max_depth=10, random_state=101,
                                        max_features = None, min_samples_leaf = 30)

            #startproc = time.time()

            # fit the model with training data
            dtree.fit(self.X_train, self.y_train)

            # predict the wine rankings for the test data set
            self.y_pred = dtree.predict(self.X_test)
            #proctime = time.time() - startproc

            return self.X_test,self.y_pred
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def calculate_confusion_matrix(self):
        # Get the confusion matrix
        return super().calculate_confusion_matrix()
    
    def save_model(self):
        return super().save_model()
    
    def get_model(self,model_id):
        return super().get_model(model_id)    
