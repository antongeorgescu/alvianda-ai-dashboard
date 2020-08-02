"""
Routes and views for dataset endpoints.
"""

from datetime import datetime
from flask import make_response, render_template, jsonify
from WineQuality_RestAPI import app
import time
import json

@app.route('/data')
def greeting():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    d = f'dataset controller accessed at {current_time}'
    return make_response(jsonify(d), 200)

@app.route('/data/ml/savedmodels')
def listsavedmodels():
    """Return saved ML models."""
    d = jsonify("model1","model2")
    return make_response(d, 200)



