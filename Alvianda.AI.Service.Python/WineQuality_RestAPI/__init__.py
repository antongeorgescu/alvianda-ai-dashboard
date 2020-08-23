"""
The flask application package.
"""

from flask import Flask
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

import WineQuality_RestAPI.views
import WineQuality_RestAPI.controllers.dataset_controller
import WineQuality_RestAPI.controllers.analytics_controller
import WineQuality_RestAPI.controllers.storage_management_controller