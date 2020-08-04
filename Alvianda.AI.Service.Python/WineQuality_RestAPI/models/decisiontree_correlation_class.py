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
    last_error = ''    
    def __init__(self, redwine_data_path, whitewine_data_path):
        try:
            self.REDWINE_PATH = redwine_data_path
            self.WHITEWINE_PATH = whitewine_data_path

            # read red wine set of observations
            data_red = pd.read_csv(self.REDWINE_PATH,sep=',')
            data_red['color'] = 1 #redwine

            # read white wine set of observations
            data_white = pd.read_csv(self.WHITEWINE_PATH,sep=',')
            data_white['color'] = 0 #whitewine
            # merge the two sets in one
            self.data = data_red.merge(data_white, how='outer')
            self.fields = list(data.columns)
        
            self.last_step_completed = 1
        except:
            self.last_error = sys.exc_info()[1]
            raise Exception(self.last_error)


    def create_histrograms():
        # show the histograms of values per feature (eg most of whines are at about 8 proof strength)
        sns.set()
        self.data.hist(figsize=(10,10),color='red', bins=13)
        plt.show()

        # show the historgram of wine rankings (quality between 1 and 10)
        self.data['quality'].hist(color='red', bins=13)
        plt.show()

    
    def reduce_dimensionality():
        # based on the "quality histograms" above, we will drop the ratings with low counts (we will keep only 3 dimesnions = 5,6,7)
        self.data = data.drop(self.data[self.data.quality == 1].index)    # not recorded anyway
        self.data = data.drop(self.data[self.data.quality == 2].index)    # not recorded anyway
        self.data = data.drop(self.data[self.data.quality == 10].index)   # not recorded anyway
        self.data = data.drop(self.data[self.data.quality == 9].index)
        self.data = data.drop(self.data[self.data.quality == 3].index)
        self.data = data.drop(self.data[self.data.quality == 8].index)
        data = data.drop(data[data.quality == 4].index)

        self.fields = list(self.data.columns)
        self.fields = list(data.columns[:-2])
        fields.append('color')  #adding color back
        self.X = data[fields]
        self.y = data['quality']

    def identify_correlations():
        # A Pearson correlation was used to identify which features correlate with wine quality. It looks as if higher the alcohol content the higher the quality. Lower density and volatile acidity also correlated with better quality as seen in the pairwise correlation chart the chart below. Only the top 5 correlated features were carried over to the KNN models.

        correlations = data[fields].corrwith(y)
        correlations.sort_values(inplace=True)

        # the following fields are the 5 retained as having the highest correlations to wine quality
        fields = correlations.map(abs).sort_values().iloc[-5:].index
        print(fields) #prints the top two abs correlations
        

        # The figure below shows Pearson Pairwise correlation of features to wine quality.
        # Looks like alcohol and density are the most correlated with quality
        ax = correlations.plot(kind='bar')
        ax.set(ylim=[-1, 1], ylabel='pearson correlation')

    def scale_dataset(scaler = 'MINMAXSCALER'):
        # two types of scalers: MINMAXSCALER, STANDARDSCALER
        X = data[fields]
        if (scaler == 'MINMAXSCALER'):
            
            scaler = MinMaxScaler()
            X = scaler.fit_transform(X)
        if (scaler == 'STANDARDSCALER'):
            # Alternative to previous scalar
            
            scaler = StandardScaler()
            # Fit on training set only.
            X = scaler.fit_transform(X)

        X = pd.DataFrame(X, columns=['%s_scaled' % fld for fld in fields])
        
    def train_and_fit_model():
        # test_size: what proportion of original data is used for test set
        X_train, X_test, y_train, y_test = train_test_split(
            X, y, test_size=1/7.0, random_state=122)

        dtree = DecisionTreeClassifier(max_depth=10, random_state=101,
                                    max_features = None, min_samples_leaf = 30)

        startproc = time.time()

        # fit the model with training data
        dtree.fit(X_train, y_train)

        # predict the wine rankings for the test data set
        y_pred = dtree.predict(X_test)
        proctime = time.time() - startproc

        print(y_pred,X_test)

    def calculate_accuracy():
        # how did our model perform?
        count_misclassified = (y_test != y_pred).sum()
        print('Misclassified samples: {}'.format(count_misclassified))
        accuracy = accuracy_score(y_test, y_pred)
        print('Accuracy: {:.2f}'.format(accuracy))

        

        # Calculate the accuracy of prediction
        # Get the accuracy score
        accuracy = accuracy_score(y_test, y_pred)
        print('Accuracy: {:.4f}'.format(accuracy))

        f1score = f1_score(y_test, y_pred, average='micro')
        print('F1_Score: {:.4f}'.format(f1score))

    def about_confusion_matrix():
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

    def calculate_confusion_matrix():
        # Get the confusion matrix
        cm = confusion_matrix(y_test, y_pred)
        print(cm)
        cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]

        sns.heatmap(cm, annot=True, fmt='.2g');
        plt.title('Confusion matrix of the KNN classifier')    
        plt.tight_layout()

    def save_model():
        summaryfile = 'ModelsFitness.txt'
        nbdir = os.getcwd()
        fsummary = open(f'{nbdir}\\{summaryfile}',"a") 
        fsummary.write('Wine Quality Analysis with Decision Tree\tProcessing (sec):{:.4f}\tAccuracy: {:.4f}\tF1-Score: {:.4f}\r\n'.format(proctime,accuracy,f1score))
        fsummary.close() 
        # save model
        return
    
    def get_model(model_id):
        return    
