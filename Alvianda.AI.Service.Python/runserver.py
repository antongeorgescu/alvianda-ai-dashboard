"""
This script runs the WQE_Service_Python application using a development server.
"""

from os import environ
from WineQuality_RestAPI import app

if __name__ == '__main__':
    try:
        # GEVENT_SUPPORT = environ.get('GEVENT_SUPPORT',True)
        HOST = environ.get('SERVER_HOST', 'localhost')
        PORT = int(environ.get('SERVER_PORT', '53535'))
        app.run(HOST, PORT, debug=True)
    except ValueError:
        print("Error getting URL from environment variables.")
    
