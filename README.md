# [2AID] Machine Learning Training & Predictor with Injectable Logic and Conformed Datasets 

## Introduction
**2AID** is an acronym for **Alvianda Artificial Intelligence Dashboard**. It is a reusable and extensible machine learning framework that uses a set of popular algorithms
to train models and enable predictions with a certain level of confidence.
This framework is configurable and requires a minimal level of customization.

## Component View
This solution offers an extenisble and reusable machine learning framework.
Following are the core components, with features and characteristics:
* Friendly user interface built as SPA
* Injectable algorithm classification & regression logic 
* Observation dataset conformity & standardization
* Data preparation (normalization and missing data management)
* Model training / re-training
* Model persistence (allows load)
* Model accuracy calculation: loss and optimization functions
* Working session management
* Exception tracking

![Alvianda 2AI](https://user-images.githubusercontent.com/6631390/93335612-b9d44a80-f7f4-11ea-89d0-6c4f1b40ff67.png)

## Data Structure View
Persistance layer relies on a relational data structure that links a set of normalized entities refelcting the natures and types of the objects used by this solution.

![2AID_Db_ERD_2](https://user-images.githubusercontent.com/6631390/93338447-58ae7600-f7f8-11ea-9fae-abfd0646f401.jpg)

## Technology
**2AID** is bult with the following technologies:
* Front-end is a user interface is a browser based single page application (SPA) built with Blazor WASM technology
* Middle-tier is a REST API that uses Python Flask module
* Persistance area manages work sessions and models and is using SqLite database

## Snapshots
Following is a set of snapshots taken from the running application (user interface):

![2aid_model_training](https://user-images.githubusercontent.com/6631390/93342479-64e90200-f7fd-11ea-8c81-d16e91ab22ee.png)
![2aid_quality_prediction](https://user-images.githubusercontent.com/6631390/93342481-65819880-f7fd-11ea-8230-487f1dfc2a09.png)
![2aid_data_preparation](https://user-images.githubusercontent.com/6631390/93342485-661a2f00-f7fd-11ea-8666-dd4157af841a.png)
![2aid_data_preparation_2](https://user-images.githubusercontent.com/6631390/93342486-661a2f00-f7fd-11ea-94bc-814ff85e80ee.png)
![2aid_dataset_viewer](https://user-images.githubusercontent.com/6631390/93342490-661a2f00-f7fd-11ea-9022-d88aeb42d636.png)

