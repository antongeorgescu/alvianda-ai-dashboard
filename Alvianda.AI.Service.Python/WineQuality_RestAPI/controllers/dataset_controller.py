"""
Routes and views for dataset endpoints.
"""

from datetime import datetime
from flask import make_response, render_template, jsonify, Response, request
from WineQuality_RestAPI import app
import time
import json

from WineQuality_RestAPI.models import winedata_class

PAGESIZE = 0

@app.route('/')
@app.route('/api/winedataset')
def startup():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    global PAGESIZE
    PAGESIZE = 25 #int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:PageSize"));

    d = f'dataset controller accessed at {current_time}'
    return make_response(jsonify(d), 200)

@app.route('/api/winedataset/entries/red')
def get_redwine_data():
    """Renders the red wine dataset, paged."""
    query_parameters = request.args
    pageno = query_parameters.get('pageno')
    
    REDWINE_PATH = "WineQuality_RestAPI/datasets/winequality-red.csv"
    
    wdata = winedata_class.WineData(REDWINE_PATH,"")
    df = wdata.redwine_data()
    df['id'] = range(len(df))
    return Response(df.to_json(orient="records"), mimetype='application/json')

@app.route('/api/winedataset/entries/white')
def get_whitewine_data():
    """Renders the red wine dataset, paged."""
    query_parameters = request.args
    pageno = query_parameters.get('pageno')

    WHITEWINE_PATH = "WineQuality_RestAPI/datasets/winequality-white.csv"
    
    wdata = winedata_class.WineData("",WHITEWINE_PATH)
    df = wdata.whitewine_data()
    df['id'] = range(len(df))
    return Response(df.to_json(orient="records"), mimetype='application/json')

@app.route('/api/winedataset/ml/savedmodels')
def listsavedmodels():
    """Return saved ML models."""
    d = jsonify("model1","model2")
    return make_response(d, 200)



