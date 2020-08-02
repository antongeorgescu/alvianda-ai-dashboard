# To add a new cell, type '# %%'
# To add a new markdown cell, type '# %% [markdown]'
# %%
import pandas as pd
import numpy as np

REDWINE_PATH = "../datasets/winequality-red.csv"
WHITEWINE_PATH = "../datasets/winequality-white.csv"

# timer start
import time 
start = time.time()

# read red wine set of observations
data_red = pd.read_csv(REDWINE_PATH,sep=',')
data_red['color'] = 1 #redwine

print(data_red.shape)

# read white wine set of observations
data_white = pd.read_csv(WHITEWINE_PATH,sep=',')
data_white['color'] = 0 #whitewine

print(data_white.shape)

# merge the two sets in one
data = data_red.merge(data_white, how='outer')
fields = list(data.columns)
print(fields)


# %%
# based on the "quality histograms", we will drop the ratings with low counts (we will keep only 5,6,7)
data = data.drop(data[data.quality == 9].index)
data = data.drop(data[data.quality == 8].index)
data = data.drop(data[data.quality == 3].index)
data = data.drop(data[data.quality == 4].index)

# show the counts of selected quality levels
print("Selected 'quality level' counts")
print(data["quality"].value_counts())


# %%
# split the data set in two: 1) color+features (observations)  2) quality (actuals)

# select the outcomes
y = data['quality']

data = data.drop(columns=['quality'])

# select the rows (observations)
fields = list(data.columns)
X = data[fields]
print(fields)

# %% [markdown]
# We will use a dimensionality reduction through Principal Component Analysis to identify only a subset of the componenta that drive the dynamics of the dataset and have other correkated dimensions with the same direction of impact but less strength. We will retain only the components that altogether amount to at least 65% of the accummulated eigen values
# 

# %%
# drop off the remaining non-chemical features.
data = data.drop(columns=['color'])
fields = list(data.columns)
print(fields)


# %%
# apply normalization to the dataset
import warnings
warnings.filterwarnings("ignore")

from sklearn.preprocessing import StandardScaler
from sklearn.preprocessing import MinMaxScaler

fields = list(X.columns)
scaler = StandardScaler().fit(X)
X = scaler.transform(X)
X = pd.DataFrame(X, columns=['%s_scaled' % fld for fld in fields])

# %% [markdown]
# The simplest way to use cross-validation is to call the cross_val_score helper function on the estimator and the dataset.
# 
# The following example demonstrates how to estimate the accuracy of a few of the algorithms we use on the wine dataset by splitting the data, fitting a model and computing the score 5 consecutive times (with different splits each time):

# %%
# define number of splits
NO_SPLITS = 5


# %%
from sklearn.model_selection import cross_val_score
from sklearn.neighbors import KNeighborsClassifier

# Instantiate KNN learning model (k=15)
knn = KNeighborsClassifier(n_neighbors=15)
# predict the wine rankings for the test data set
scores = cross_val_score(knn, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%
from sklearn import svm

# Instantiate SVM learning model
clf = svm.SVC(kernel='linear', C=1)
scores = cross_val_score(clf, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%
from sklearn.linear_model import LogisticRegression

MYSOLVER = "lbfgs" #"newton-cg"

logreg = LogisticRegression(C=1.0, class_weight=None, dual=False, fit_intercept=True,
          intercept_scaling=1, max_iter=100, multi_class='auto',
          n_jobs=None, penalty='l2', random_state=None, solver=MYSOLVER,
          tol=0.0001, verbose=0, warm_start=False)
scores = cross_val_score(logreg, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%
from sklearn.ensemble import RandomForestClassifier
ranfc = RandomForestClassifier(n_estimators=70, oob_score=True, n_jobs=-1,
                            random_state=101, max_features = None, min_samples_leaf = 30)
scores = cross_val_score(ranfc, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%
from sklearn.tree import DecisionTreeClassifier

dtree = DecisionTreeClassifier(max_depth=10, random_state=101,
                            max_features = None, min_samples_leaf = 30)
scores = cross_val_score(dtree, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%
from sklearn.naive_bayes import GaussianNB

nb = GaussianNB()
scores = cross_val_score(nb, X, y, cv=NO_SPLITS)
print(scores)
print("Accuracy: %0.2f (+/- %0.2f)" % (scores.mean(), scores.std() * 2))


# %%



