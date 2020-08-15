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

class DataPreparationSingleton:
    class __DataPreparationSingleton:
        def __init__(self,redwine_dataset_path = None,whitewine_dataset_path = None):
            try:
                if not((redwine_dataset_path is None) and (whitewine_dataset_path is None)):
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
        def __create_histrograms__(self):
            chart_path = f'{os.getcwd()}/WineQuality_RestAPI/static'
            try:
                # show the histograms of values per feature (eg most of whines are at about 8 proof strength)
                num_attributes = 13
                sns.set()
                self.merged_data.hist(figsize=(10,10),color='red', bins=num_attributes)
                plt.savefig('{}/attribute_histogram.jpg'.format(chart_path))

                plt.cla()

                quality_range = 10
                x2 = self.merged_data['quality'].rename_axis('ID').values
                plt.figure()
                n, bins, patches = plt.hist(x2, quality_range, facecolor='red', alpha=0.5)
                plt.xlabel('Quality (1=low,10=high')
                plt.ylabel('# Wines')
                plt.savefig('{}/quality_histogram.jpg'.format(chart_path))

                return "attribute_histogram.jpg,quality_histogram.jpg","Wine Attributes Histogram,Wine Quality Histogram"
            except:
                self.last_error = sys.exc_info()[1]
                raise Exception(self.last_error)
        def __reduce_dimensionality__(self):
            try:
                list_quality_drop = []

                # based on the "quality histograms" above, we will drop the ratings with low counts (we will keep only 3 dimesnions = 5,6,7)
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 1].index)    # not recorded anyway
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 2].index)    # not recorded anyway
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 10].index)   # not recorded anyway
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 9].index)
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 3].index)
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 8].index)
                self.merged_data = self.merged_data.drop(self.merged_data[self.merged_data.quality == 4].index)

                list_quality_drop.append(1)
                list_quality_drop.append(2)
                list_quality_drop.append(10)

                self.fields = list(self.merged_data.columns)
                self.fields = list(self.merged_data.columns[:-2])
                self.fields.append('color')  #adding color back
                self.X = self.merged_data[self.fields]
                self.y = self.merged_data['quality']
                separator=','
                return separator.join(map(str,list_quality_drop))
            except:
                self.last_error = sys.exc_info()[1]
                raise Exception(self.last_error)
        def __identify_correlations__(self):
            # A Pearson correlation was used to identify which features correlate with wine quality. It looks as if higher the alcohol content the higher the quality. Lower density and volatile acidity also correlated with better quality as seen in the pairwise correlation chart the chart below. Only the top 5 correlated features were carried over to the KNN models.
            chart_path = f'{os.getcwd()}/WineQuality_RestAPI/static'
            try:
                correlations = self.merged_data[self.fields].corrwith(self.y)
                correlations.sort_values(inplace=True)

                # the following fields are the 5 retained as having the highest correlations to wine quality
                self.fields = correlations.map(abs).sort_values().iloc[-5:].index
                # print(self.fields) #prints the top two abs correlations
        
                # The figure below shows Pearson Pairwise correlation of features to wine quality.
                # Looks like alcohol and density are the most correlated with quality
                fig, ax = plt.subplots()
                ax = correlations.plot(kind='bar')
                ax.set(ylim=[-1, 1], ylabel='pearson correlation')
                #plt.show()
                plt.savefig('{}/attribute_correlations.jpg'.format(chart_path))

                return ','.join(self.fields),"attribute_correlations.jpg","Wine Attribute Correlations (first 5)"
            except:
                self.last_error = sys.exc_info()[1]
                raise Exception(self.last_error)
        def __get_observations_and_labels__(self):
            return self.X, self.y
    instance = None
    def __init__(self,redwine_dataset_path = None,whitewine_dataset_path = None):
        if not DataPreparationSingleton.instance:
            DataPreparationSingleton.instance = DataPreparationSingleton.__DataPreparationSingleton(redwine_dataset_path,whitewine_dataset_path)
    def create_histrograms(self):
        return self.instance.__create_histrograms__()
    def reduce_dimensionality(self):
        return self.instance.__reduce_dimensionality__()
    def identify_correlations(self):
        return self.instance.__identify_correlations__()
    def get_observations_and_labels(self):
        return self.instance.__get_observations_and_labels__()
    def test_create_histrograms(self):
        chart_path = f'{os.getcwd()}/WineQuality_RestAPI/static'

        df = pd.DataFrame(np.array([21,22,23,4,5,6,77,8,9,10,31,32,33,34,35,36,37,18,49,50,100]),
                           columns=['a'])
        x = df.rename_axis('ID').values
        # x = [21,22,23,4,5,6,77,8,9,10,31,32,33,34,35,36,37,18,49,50,100]
        num_bins = 5
        n, bins, patches = plt.hist(x, num_bins, facecolor='blue', alpha=0.5)
        #plt.show()
        plt.savefig('{}/file1.jpg'.format(chart_path)) 

        plt.cla()

        df = pd.DataFrame(np.array([81,82,83,124,125,136,77,48,49,50,41,42,43,44,45,46,47,18,49,50,100]),
                           columns=['b'])
        x2 = df.rename_axis('ID').values
        # x2 = [81,82,83,124,125,136,77,48,49,50,41,42,43,44,45,46,47,18,49,50,100]
        num_bins = 5
        n, bins, patches = plt.hist(x2, num_bins, facecolor='red', alpha=0.5)
        #plt.show()
        plt.savefig('{}/file2.jpg'.format(chart_path)) 

        return "file1.jpg,file2.jpg","Histogram #1,Histogram #2"

    def test_plot_result_data(self,acc_total, acc_val_total, loss_total, losss_val_total, epoch):
        chart_path = f'{os.getcwd()}/WineQuality_RestAPI/static'

        y = range(epoch)
        plt.plot(y,acc_total,linestyle="-",  linewidth=1,label='acc_train')
        plt.plot(y,acc_val_total,linestyle="-", linewidth=1,label='acc_val')
        plt.legend(('acc_train', 'acc_val'), loc='upper right')
        plt.xlabel("Training Epoch")
        plt.ylabel("Acc on dataset")
        plt.savefig('{}/acc.png'.format(chart_path))
        plt.cla()
        plt.plot(y,loss_total,linestyle="-", linewidth=1,label='loss_train')
        plt.plot(y,losss_val_total,linestyle="-", linewidth=1,label='loss_val')
        plt.legend(('loss_train', 'loss_val'), loc='upper right')
        plt.xlabel("Training Epoch")
        plt.ylabel("Loss on dataset")
        plt.savefig('{}/loss.png'.format(chart_path)) 

        return "acc.png,loss.png","Accuracy Rate,Loss Rate"