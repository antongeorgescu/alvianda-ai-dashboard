"""
The flask application package.
"""

from flask import Flask
from flask_cors import CORS
from flask_session import Session
import os

app = Flask(__name__)
app.secret_key = os.urandom(24)
CORS(app)

import WineQuality_RestAPI.views
import WineQuality_RestAPI.controllers.dataset_process_controller
import WineQuality_RestAPI.controllers.wine_analytics_controller
import WineQuality_RestAPI.controllers.storage_management_controller
import WineQuality_RestAPI.controllers.workbench_test_controller