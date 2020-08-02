"""
The flask application package.
"""

from flask import Flask
app = Flask(__name__)

import WineQuality_RestAPI.views
import WineQuality_RestAPI.controllers.dataset_controller