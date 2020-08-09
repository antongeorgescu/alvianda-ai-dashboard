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
    def __init__(self,merged_dataset,field_list):
        merged_data = merged_dataset
        fields = field_list
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

            self.X = pd.DataFrame(X, columns=['%s_scaled' % fld for fld in self.fields])
            return self.X
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def about_confusion_matrix(self):
        # ### About Confusion Matrix
        # 
        # A confusion matrix is a table that is often used to describe the performance of a classification model (or "classifier") 
        # on a set of test data for which the true values are known.
        # 
        # Here is a set of the most basic terms, which are whole numbers (not rates):
        # 1. true positives (TP): These are cases in which we predicted "yes" where it is actually "yes".
        # 2. true negatives (TN): We predicted no, and it is actually "no".
        # 3. false positives (FP): We predicted "yes", and was actually "no" (Also known as a "Type I error."
        # 4. false negatives (FN): We predicted "no", and was actually "yes". (Also known as a "Type II error.")
        #     
        # This is a list of rates that are often computed from a confusion matrix for a binary classifier:
        # 
        # **Accuracy**: Overall, how often is the classifier correct? 
        #     <br>&emsp;accuracy = (TP+TN)/total
        # 
        # **Misclassification Rate**: Overall, how often is it wrong? 
        #     <br>&emsp;misrate = (FP+FN)/total
        #     <br>&emsp;equivalent to 1 minus Accuracy
        #     <br>&emsp;also known as "Error Rate"
        # 
        # **True Positive Rate**: When it's actually yes, how often does it predict yes? 
        #     <br>&emsp;tprate = TP/actual_yes
        #     <br>&emsp;also known as "Sensitivity" or "Recall"
        # 
        # **False Positive Rate**: When it's actually no, how often does it predict yes?
        #     <br>&emsp;fprate = FP/actual_no
        # 
        # **True Negative Rate**: When it's actually no, how often does it predict no? 
        #     <br>&emsp;tnrate = TN/actual_no
        #     <br>&emsp;equivalent to 1 minus False Positive Rate
        #     <br>&emsp;also known as "Specificity"
        # 
        # **Precision**: When it predicts yes, how often is it correct? 
        #     <br>&emsp;precision = TP/predicted_yes
        # 
        # **Prevalence**: How often does the yes condition actually occur in our sample? 
        #     <br>&emsp;prevalence = actual_yes/total
        #     
        # ![Confusion Matrix - Theoretical Foundation](https://raw.githubusercontent.com/antongeorgescu/machine-learning-documentation/master/images/Confusion-Matrix-2.PNG) 
        return ""

    def calculate_confusion_matrix(self):
        # Get the confusion matrix
        
        try:
            cm = confusion_matrix(self.y_test, self.y_pred)
            print(cm)
            cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]

            sns.heatmap(cm, annot=True, fmt='.2g');
            plt.title('Confusion matrix of the KNN classifier')    
            plt.tight_layout()
            return ""
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def calculate_accuracy(self):
        # how did our model perform?
        
        try:
            count_misclassified = (self.y_test != self.y_pred).sum()
            print('Misclassified samples: {}'.format(count_misclassified))
            accuracy = accuracy_score(self.y_test, self.y_pred)
            print('Accuracy: {:.2f}'.format(accuracy))
        
            # Calculate the accuracy of prediction
            # Get the accuracy score
            accuracy = accuracy_score(self.y_test, self.y_pred)
            print('Accuracy: {:.4f}'.format(accuracy))

            f1score = f1_score(self.y_test, self.y_pred, average='micro')
            print('F1_Score: {:.4f}'.format(f1score))
            return ""
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def save_model(self):
        try:
            summaryfile = 'ModelsFitness.txt'
            nbdir = os.getcwd()
            fsummary = open(f'{nbdir}\\{summaryfile}',"a") 
            fsummary.write('Wine Quality Analysis with Decision Tree\tProcessing (sec):{:.4f}\tAccuracy: {:.4f}\tF1-Score: {:.4f}\r\n'.format(proctime,accuracy,f1score))
            fsummary.close() 
            # save model
            return
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)
    
    def get_model(self,model_id):
        return   
