#!/bin/sh
redo-ifchange generate-energyplus-field-key-data.msh 25.2_Energy+.json energyplus-idd.schema.json
mshell generate-energyplus-field-key-data.msh 25.2_Energy+.json energyplus-idd.schema.json "$3"
