import pandas as pd
import os

class WineData:
    redwine_dataset = ''
    whitewine_dataset = ''
    def __init__(self,redwine_filepath,whitewine_filepath):
        self.redwine_dataset = redwine_filepath
        self.whitewine_dataset = whitewine_filepath

    def datasets_lengths(self):
        data_red = pd.read_csv(self.redwine_dataset,sep=',')
        data_white = pd.read_csv(self.whitewine_dataset,sep=',')
        return len(data_red.index),len(data_white.index)

    def redwine_data(self):
        data_red = pd.read_csv(self.redwine_dataset,sep=',')
        return data_red

    def whitewine_data(self):
        data_white = pd.read_csv(self.whitewine_dataset,sep=',')
        return data_white