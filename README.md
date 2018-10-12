<p align="left"><img src="Banner.svg" width="300"></p>

EC Fan Offset Controller for Clevo Notebooks


Uses [RW Everything](http://rweverything.com/) to adjust the embedded controller flags to somewhat adjust the fan curve of Clevo/Sager/Metabox notebooks. 

Currently only supports the P650RE, but if you're willing to find your own data then the offsets and calculations can be modified in Settings.xml (generated on first launch). 

![](https://i.imgur.com/lruD3I7.png)

RWEverything must be downloaded first. 

**Notes**
* Modifying the EC can be dangerous. If in doubt, it's best to err on the side of caution and avoid any changes. 
* I recommend checking that your EC fan control is at the right offset in RW everything first, to ensure that the right control is being changed. Use [this document](http://forum.notebookreview.com/threads/successful-p650re-fan-speed-modification-via-ec.807214/#post-10572037) to play around with first so you understand what changes will happen. 
* A good sign that everything is likely to work is if ECFan correctly shows your RPM and CPU temperature; these values are read from the EC directly so if they're correct, the fan control offset is also most likely correct. 


**Planned Features**
* Allow saving of fan profiles
* Auto-enable boosted fan speeds when gameplay is detected
