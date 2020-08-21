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

class BaseAlgorithmClass:
    merged_data = None
    fields = None
    y_test = None
    y_pred = None
    trainmodel = None

    def __init__(self,merged_dataset):
        self.merged_data = merged_dataset
        self.fields = list(self.merged_data.columns[1:-2])
        self.fields.append('color')  #adding color back
    
    def scale_dataset(self,scaler = 'MINMAXSCALER'):
        # two types of scalers: MINMAXSCALER, STANDARDSCALER
        try:
            self.X = self.merged_data[self.fields]
            if (scaler == 'MINMAXSCALER'):
                scaler = MinMaxScaler()
                self.X = scaler.fit_transform(self.X)
            if (scaler == 'STANDARDSCALER'):
                # Alternative to previous scalar
            
                scaler = StandardScaler()
                # Fit on training set only.
                self.X = scaler.fit_transform(self.X)

            self.X = pd.DataFrame(self.X, columns=['%s_scaled' % fld for fld in self.fields])
            return self.X
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)
    
    # def about_confusion_matrix(self):
    #     about_file_path = f'{os.getcwd()}/WineQuality_RestAPI/models/about_confusion_matrix.txt'
    #     f = open(about_file_path, "r")
    #     return f.read()
    
    def calculate_confusion_matrix(self):
        # Get the confusion matrix
        rundata = None
        try:
            cm = confusion_matrix(self.y_test, self.y_pred)
            print(cm)
            rundata = cm
            cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]

            sns.heatmap(cm, annot=True, fmt='.2g')
            plt.title('Confusion matrix of the KNN classifier')    
            plt.tight_layout()
            return rundata
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def calculate_accuracy(self):
        # how did our model perform?
        rundata = None
        try:
            count_misclassified = (self.y_test != self.y_pred).sum()
            print('Misclassified samples: {}'.format(count_misclassified))
            rundata = 'Misclassified samples: {}'.format(count_misclassified)
            
            accuracy = accuracy_score(self.y_test, self.y_pred)
            print('Accuracy: {:.2f}'.format(accuracy))
            rundata += ', Accuracy: {:.2f}'.format(accuracy)
        
            f1score = f1_score(self.y_test, self.y_pred, average='micro')
            print('F1_Score: {:.4f}'.format(f1score))
            rundata += ', F1_Score: {:.4f}'.format(f1score)

            return rundata
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def set_model(self,model,model_name):
        self.trainmodel = model
        self.modelname = model_name 

    def get_model(self):
        return self.trainmodel, self.modelname
