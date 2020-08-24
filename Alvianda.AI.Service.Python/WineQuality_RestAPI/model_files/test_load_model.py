import pandas
from sklearn import model_selection
from sklearn.linear_model import LogisticRegression
import pickle
import os, numpy, csv


# read X_test[0:5]
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test.csv'
X_test = numpy.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)

# X_test = []
# with open(filename) as f:
#     reader = csv.reader(f)
#     for row in list(reader):
#         X_test.append([float(x) for x in row])

# read Y_test[0:5]
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test.csv'
Y_test = numpy.loadtxt(open(filename, "rb"), delimiter=",", skiprows=0)

# Y_test = []
# with open(filename) as f:
#     reader = csv.reader(f)
#     for row in list(reader):
#         Y_test.append([float(x) for x in row])
    
# load the model from disk
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/saved_model.pkl'
loaded_model = pickle.load(open(filename, 'rb'))
score = loaded_model.score(X_test, Y_test)

prediction = loaded_model.predict(X_test)

print(f'Score:{score},Predictions:{prediction}')