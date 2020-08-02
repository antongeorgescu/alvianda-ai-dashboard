"""
Routes and views for dataset endpoints.
"""

from datetime import datetime
from flask import make_response, render_template, jsonify, Response
from WineQuality_RestAPI import app
import time
import json

from WineQuality_RestAPI.models import winedata_class

@app.route('api/winedataset')
def greeting():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    d = f'dataset controller accessed at {current_time}'
    return make_response(jsonify(d), 200)

@app.route('api/winedataset/entries/redwine')
def get_redwine_data():
    """Renders the red wine dataset."""

    REDWINE_PATH = "WineQuality_RestAPI/datasets/winequality-red.csv"
    
    wdata = winedata_class.WineData(REDWINE_PATH,"")
    df = wdata.redwine_data()
    return Response(df.to_json(orient="records"), mimetype='application/json')

@app.route('api/winedataset/entries/whitewine')
def get_whitewine_data():
    """Renders the red wine dataset."""

    WHITEWINE_PATH = "WineQuality_RestAPI/datasets/winequality-white.csv"
    
    wdata = winedata_class.WineData("",WHITEWINE_PATH)
    df = wdata.whitewine_data()
    return Response(df.to_json(orient="records"), mimetype='application/json')

@app.route('api/winedataset/ml/savedmodels')
def listsavedmodels():
    """Return saved ML models."""
    d = jsonify("model1","model2")
    return make_response(d, 200)



