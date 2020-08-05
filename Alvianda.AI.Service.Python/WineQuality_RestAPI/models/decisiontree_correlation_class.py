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

class DecisionTreeAnalyzer:
    def __init__(self,redwine_dataset_path,whitewine_dataset_path):
        try:
            # read red wine set of observations
            data_red = pd.read_csv(redwine_dataset_path,sep=',')
            data_red['color'] = 1 #redwine

            # read white wine set of observations
            data_white = pd.read_csv(whitewine_dataset_path,sep=',')
            data_white['color'] = 0 #whitewine
            # merge the two sets in one
            self.merged_data = data_red.merge(data_white, how='outer')
            self.fields = list(self.merged_data.columns)
       
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)


    def create_histrograms(self):
        try:
            # show the histograms of values per feature (eg most of whines are at about 8 proof strength)
            sns.set()
            self.merged_data.hist(figsize=(10,10),color='red', bins=13)
            plt.savefig(f'{os.getcwd()}/WineQuality_RestAPI/static/attribute_historgram.jpg')

            # show the historgram of wine rankings (quality between 1 and 10)
            self.merged_data['quality'].hist(color='red', bins=13)
            plt.savefig(f'{os.getcwd()}/WineQuality_RestAPI/static/quality_histogram.jpg')

            return "attribute_historgram.jpg,quality_histogram.jpg","Wine Attributes Histogram,Wine Quality Histogram"
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    
    def reduce_dimensionality(self):
        try:
            # based on the "quality histograms" above, we will drop the ratings with low counts (we will keep only 3 dimesnions = 5,6,7)
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 1].index)    # not recorded anyway
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 2].index)    # not recorded anyway
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 10].index)   # not recorded anyway
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 9].index)
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 3].index)
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 8].index)
            self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 4].index)

            self.fields = list(self.merged_data.columns)
            self.fields = list(self.merged_data.columns[:-2])
            self.fields.append('color')  #adding color back
            self.X = self.merged_data[self.fields]
            self.y = self.merged_data['quality']
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def identify_correlations(self):
        # A Pearson correlation was used to identify which features correlate with wine quality. It looks as if higher the alcohol content the higher the quality. Lower density and volatile acidity also correlated with better quality as seen in the pairwise correlation chart the chart below. Only the top 5 correlated features were carried over to the KNN models.

        try:
            correlations = self.merged_data[self.fields].corrwith(self.y)
            correlations.sort_values(inplace=True)

            # the following fields are the 5 retained as having the highest correlations to wine quality
            self.fields = correlations.map(abs).sort_values().iloc[-5:].index
            print(self.fields) #prints the top two abs correlations
        

            # The figure below shows Pearson Pairwise correlation of features to wine quality.
            # Looks like alcohol and density are the most correlated with quality
            ax = correlations.plot(kind='bar')
            ax.set(ylim=[-1, 1], ylabel='pearson correlation')
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

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
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)

    def train_and_fit_model(self):
        # test_size: what proportion of original data is used for test set
        
        try:
            self.X_train, self.X_test, self.y_train, self.y_test = train_test_split(
                self.X, self.y, test_size=1/7.0, random_state=122)

            dtree = DecisionTreeClassifier(max_depth=10, random_state=101,
                                        max_features = None, min_samples_leaf = 30)

            startproc = time.time()

            # fit the model with training data
            dtree.fit(self.X_train, self.y_train)

            # predict the wine rankings for the test data set
            self.y_pred = dtree.predict(self.X_test)
            proctime = time.time() - startproc

            print(self.y_pred,self.X_test)
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
        return

    def calculate_confusion_matrix(self):
        # Get the confusion matrix
        
        try:
            cm = confusion_matrix(self.y_test, self.y_pred)
            print(cm)
            cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]

            sns.heatmap(cm, annot=True, fmt='.2g');
            plt.title('Confusion matrix of the KNN classifier')    
            plt.tight_layout()
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
