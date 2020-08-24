import pandas
from sklearn import model_selection
from sklearn.linear_model import LogisticRegression
import pickle
import os
import numpy

url = "https://raw.githubusercontent.com/jbrownlee/Datasets/master/pima-indians-diabetes.data.csv"
names = ['preg', 'plas', 'pres', 'skin', 'test', 'mass', 'pedi', 'age', 'class']
dataframe = pandas.read_csv(url, names=names)
array = dataframe.values
X = array[:,0:8]
Y = array[:,8]
test_size = 0.33
seed = 7
X_train, X_test, Y_train, Y_test = model_selection.train_test_split(X, Y, test_size=test_size, random_state=seed)
# Fit the model on training set
model = LogisticRegression()
model.fit(X_train, Y_train)

print(X_test[0:5],Y_test[0:5])

# save X_test[0:5]
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_X_test.csv'
numpy.savetxt(filename, X_test[0:5], delimiter=",")

# save Y_test[0:5]
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/test_saved_Y_test.csv'
numpy.savetxt(filename, Y_test[0:5], delimiter=",")

#prediction = model.predict([X_test[0],X_test[1],X_test[2]])
# save the model to disk
filename = f'{os.getcwd()}/WineQuality_RestAPI/model_files/saved_model.pkl'
pickle.dump(model, open(filename, 'wb'))
