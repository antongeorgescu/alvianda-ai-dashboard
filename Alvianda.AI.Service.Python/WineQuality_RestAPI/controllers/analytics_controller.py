"""
Routes and views for analytics endpoints.
"""

from requests import Session
from signalr import Connection
from datetime import datetime
from flask import make_response, render_template, jsonify, Response, request
from WineQuality_RestAPI import app
import time
import json

from WineQuality_RestAPI.models.decisiontree_correlation_class import DecisionTreeAnalyzer

@app.route('/api/wineanalytics/validate')
def validate():
    """Renders the controller greeting."""
    t = time.localtime()
    current_time = time.strftime("%D %H:%M:%S", t)
    
    d = f'analytics controller accessed at {current_time}'
    return make_response(jsonify(d), 200)

@app.route('/api/wineanalytics/runanalyzer')
def run_analysis():
    query_parameters = request.args
    algorithm = query_parameters.get('algorithm')
    
    REDWINE_PATH = "../datasets/winequality-red.csv"
    WHITEWINE_PATH = "../datasets/winequality-white.csv"

    rundata = None  # should be a dictionary
    dtanalyzer = None

    try:
        if (algorithm == "decision-tree"):
            dtanalyzer = DecisionTreeAnalyzer(REDWINE_PATH,WHITEWINE_PATH)  
            dtanalyzer.create_histrograms()
            dtanalyzer.reduce_dimensionality()
            dtanalyzer.identify_correlations()
            dtanalyzer.scale_dataset()
            dtanalyzer.train_and_fit_model()
            dtanalyzer.calculate_accuracy()
            dtanalyzer.calculate_confusion_matrix()
            dtanalyzer.save_model()
    except Exception as error:
        return make_response(error,500)

    return make_response(jsonify(rundata),200)

@app.route('/api/wineanalytics/chathub')
def chat():
    with Session() as session:
        #create a connection
        connection = Connection("http://localhost:33644/wineanalytics", session)

        #get chat hub
        chat = connection.register_hub('chat')

        #start a connection
        connection.start()

        #create new chat message handler
        def print_received_message(data):
            print('received: ', data)

        #create new chat topic handler
        def print_topic(topic, user):
            print('topic: ', topic, user)

        #create error handler
        def print_error(error):
            print('error: ', error)

        #receive new chat messages from the hub
        chat.client.on('newMessageReceived', print_received_message)

        #change chat topic
        chat.client.on('topicChanged', print_topic)

        #process errors
        connection.error += print_error

        #start connection, optionally can be connection.start()
        with connection:

            #post new message
            chat.server.invoke('send', 'Python is here')

            #change chat topic
            chat.server.invoke('setTopic', 'Welcome python!')

            #invoke server method that throws error
            chat.server.invoke('requestError')

            #post another message
            chat.server.invoke('send', 'Bye-bye!')

            #wait a second before exit
            connection.wait(1)