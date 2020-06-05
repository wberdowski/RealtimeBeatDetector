# Realtime Beat Detector
![screenshot](https://i.imgur.com/Iv8u6oG.png)

Realtime Beat Detector is a WinForms application that uses simple sound energy based beat detection algorithm.
Audio input is provided by the WASAPI loopback capture interface included in the [CsCore](https://github.com/filoe/cscore) library.
The BPM displayed in the center is an average value from the last 4 measurements.
