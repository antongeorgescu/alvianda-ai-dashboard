"""
Routes and views for dataset endpoints.
"""

from datetime import datetime
from flask import make_response, render_template, jsonify, Response, request
from WineQuality_RestAPI import app
import time
import json

from WineQuality_RestAPI.models import winedata_class

PAGESIZE = 25
TOTALRECORDS_RED = 0;
TOTALRECORDS_WHITE = 0;

@app.route('/')
@app.route('/api/winedataset')
def startup():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    d = f'dataset controller accessed at {current_time}'
    return make_response(jsonify(d), 200)

@app.route('/api/winedataset/settings')
def settings():
    """Renders the pagination settings."""
    global PAGESIZE, TOTALRECORDS_RED, TOTALRECORDS_WHITE

    REDWINE_PATH = "WineQuality_RestAPI/datasets/winequality-red.csv"
    WHITEWINE_PATH = "WineQuality_RestAPI/datasets/winequality-white.csv"

    wineobj = winedata_class.WineData(REDWINE_PATH,WHITEWINE_PATH);
    TOTALRECORDS_RED, TOTALRECORDS_WHITE = wineobj.datasets_lengths()
    
    settings = {
        "pageSize":f'{PAGESIZE}',
        "totalRecsRed":f'{TOTALRECORDS_RED}',
        "totalRecsWhite":f'{TOTALRECORDS_WHITE}'
    }

    return make_response(json.dumps(settings), 200)

@app.route('/api/winedataset/entries/red')
def get_redwine_data():
    """Renders the red wine dataset, paged."""
    query_parameters = request.args
    pageno = int(query_parameters.get('pageno'))-1
    
    REDWINE_PATH = "WineQuality_RestAPI/datasets/winequality-red.csv"
    
    wdata = winedata_class.WineData(REDWINE_PATH,"")
    df = wdata.redwine_data()
    df['id'] = range(len(df))
    df = df[["id","fixed acidity","volatile acidity","citric acid","residual sugar","chlorides","free sulfur dioxide","total sulfur dioxide","density","pH","sulphates","alcohol","quality"]]
    dfret = df.to_dict('records')
    dfret = df.iloc[pageno*PAGESIZE:(pageno+ 1)*PAGESIZE].to_dict('records')
    #return Response(dfret.to_json(orient="records"), mimetype='application/json')
    #return Response(jsonify(dfret),mimetype='application/json')
    return make_response(jsonify(dfret),200)

@app.route('/api/winedataset/entries/white')
def get_whitewine_data():
    """Renders the red wine dataset, paged."""
    query_parameters = request.args
    pageno = int(query_parameters.get('pageno'))-1

    WHITEWINE_PATH = "WineQuality_RestAPI/datasets/winequality-white.csv"
    
    wdata = winedata_class.WineData("",WHITEWINE_PATH)
    df = wdata.whitewine_data()
    df['id'] = range(len(df))
    df = df[["id","fixed acidity","volatile acidity","citric acid","residual sugar","chlorides","free sulfur dioxide","total sulfur dioxide","density","pH","sulphates","alcohol","quality"]]
    dfret = df.iloc[pageno*PAGESIZE:(pageno+ 1)*PAGESIZE].to_dict('records')
    return make_response(jsonify(dfret),200)

@app.route('/api/winedataset/ml/savedmodels')
def listsavedmodels():
    """Return saved ML models."""
    d = jsonify("model1","model2")
    return make_response(d, 200)



