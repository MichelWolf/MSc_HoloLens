import astropy.units as u
import astropy.coordinates as coord
from astroquery.gaia import Gaia
from astropy.io import ascii
import numpy as np
import pandas as pd
import math
import struct
from functools import partial
import sys
import os
from os import listdir

import warnings
warnings.filterwarnings("ignore")

pd.set_option("display.max_rows", None, "display.max_columns", None)

maxSelection = '100'
columns = 'source_id, ra, dec, pmra, pmdec, parallax, teff_val, radius_val, designation'
minParallax = '0.02'
maxParallax = '0.03'
parallaxOverError = '10'
coordinateFile = 'coordinates100.bin'
parsedValuesFile = 'parsedValues100000.bin'

parsecPerSlice = 2600

queryBase = """SELECT TOP {maxSelection}
{columns}
FROM gaiadr2.gaia_source
WHERE parallax_over_error > 10 AND 
parallax > {minParallax} AND
parallax < 1000"""# AND"""
#teff_val IS NOT null
#"""

queryBase3 = """SELECT TOP {maxSelection}
{columns}
FROM gaiadr2.gaia_source
WHERE (parallax BETWEEN {minParallax} AND {maxParallax}) AND
parallax_over_error > {parallaxOverError} AND
(gaiadr2.gaia_source.radius_val>=2.0)"""# AND"""
#teff_val IS NOT null
#"""

queryBase2 = """SELECT TOP {maxSelection} {columns}
FROM gaiadr2.gaia_source 
WHERE 
CONTAINS(
	POINT('ICRS',gaiadr2.gaia_source.ra,gaiadr2.gaia_source.dec),
	CIRCLE('ICRS',266.4051,-28.936175,5)
)=1  AND  (gaiadr2.gaia_source.parallax>={minParallax})"""#AND teff_val IS NOT null
#"""

queryBase4 = """SELECT *
FROM gaiadr2.gaia_source AS G, gaiadr2.vari_cepheid AS V WHERE G.source_id=V.source_id AND
parallax > 0
"""

queryBase5 = """
SELECT TOP {maxSelection}
{columns}
FROM gaiadr2.gaia_source 
WHERE (gaiadr2.gaia_source.radius_val>=2.0)
"""

queryBase6 = """
SELECT {columns}
FROM gaiadr2.gaia_source 
WHERE (gaiadr2.gaia_source.random_index<=1000000 AND gaiadr2.gaia_source.parallax BETWEEN 0.0384615384615385 AND 1000 AND teff_val > 0.1)
"""



maxX = 0
maxY = 0
maxZ = 0
maxValue = 0
queriedStars = 0

def getTableMeta():
    meta = Gaia.load_table('gaiadr2.gaia_source')
    print(meta)
    
    for column in (meta.columns):
        print(column.name)



def synchronousQuery():
    query1 = """SELECT 
    TOP 10
    source_id, ref_epoch, ra, dec, parallax 
    FROM gaiadr2.gaia_source"""

    job1 = Gaia.launch_job(query1)
    print(job1)
    
    results1 = job1.get_results()
    results1.pprint_all()

def asyncQuery():
    query2 = """SELECT TOP 3000
    source_id, ref_epoch, ra, dec, parallax
    FROM gaiadr2.gaia_source
    WHERE parallax < 1
    """
    job2 = Gaia.launch_job_async(query2)
    print(job2)
    results2 = job2.get_results()
    results2.pprint_all()
    
    Gaia.remove_jobs([job2.jobid])


def appendToFile(f, x, y, z, temper, distance, radius, source_id):
    
    s = struct.pack('fffiffq', x, y, z, temper, distance, radius, source_id)
    f.write(s)


def handleResults(f, results2):
    count = 0
    
    temper = results2["teff_val"]
    print("Min K: {0} - Max K: {1}".format(min(temper),max(temper)))
    dist = coord.Distance(parallax=u.Quantity(results2["parallax"]))
    
    print("Min Parallax: {0} - Max Parallax: {1}".format(min(dist),max(dist)))
    print(len(results2))
    
    for row in results2:
        dist = coord.Distance(parallax=u.Quantity(row["parallax"] * u.mas))
        
        c = coord.SkyCoord(ra=row["ra"] * u.deg,
                   dec=row["dec"] * u.deg,
                   distance=dist)

        c = c.cartesian

        radius = float(row["radius_val"])

        if math.isnan(radius):
            radius = float(0)
        source_id = np.longlong(row["source_id"])

        print (c.x.value / 26000.0)
        print (c.y.value/ 26000.0)
        print (c.z.value/ 26000.0)
        x = np.single(c.x.value)
        y = np.single(c.y.value)
        z = np.single(c.z.value)

        global maxX
        global maxY
        global maxZ

        if abs(x) > maxX:
            maxX = abs(x)

        if abs(y) > maxY:
            maxY = abs(y)

        if abs(z) > maxZ:
            maxZ = abs(z)

        temper = math.floor(row["teff_val"])

        appendToFile(f, x, y, z, temper, float(dist.value), radius, source_id)
        count = count+1
        sys.stdout.write("\rFortschritt: " + str(count) + "/" + str(maxSelection))
        
    global maxValue
    global queriedStars
    queriedStars = queriedStars + count
    maxValue = max(maxX, maxY, maxZ)


def parseValues(maxValue):
    f = open(parsedValuesFile, 'wb')
    f.close()
    f = open(parsedValuesFile, 'ab')
    print(maxValue)
    global queriedStars
    count = 0

    s = struct.pack('i', np.int32(queriedStars))
    f.write(s)
    print(coordinateFile)
    


    with open(coordinateFile, 'rb') as openfileobject:
        chunk = openfileobject.read(32)
        while chunk:
            unpackedStruct = struct.unpack('fffiffq',chunk)
            parsedX = np.single(unpackedStruct[0] / maxValue)
            parsedY = np.single(unpackedStruct[1] / maxValue)
            parsedZ = np.single(unpackedStruct[2] / maxValue)
            appendToFile(f, parsedX, parsedY, parsedZ, unpackedStruct[3], unpackedStruct[4], unpackedStruct[5],unpackedStruct[6])
            count = count+1
            sys.stdout.write("\rFortschritt: " + str(count) + "/" + str(maxSelection))

            chunk = openfileobject.read(32)
            
  


    f.close()

def main():
    #maxSelection = input("How many stars would you like to query? ")

    query6 = queryBase6.format(columns=columns)
    job2 = Gaia.launch_job_async(query6)
    results2 = job2.get_results()

    global maxSelection
    maxSelection = str(len(results2) + 1)
    print(maxSelection)

    global coordinateFile
    global parsedValuesFile
    coordinateFile = ("coordinates" + maxSelection + ".bin") 
    parsedValuesFile = ("parsedValues" + maxSelection + ".bin") 
    #Resets coordinateFile to an empty file
    f = open(coordinateFile, 'wb')
    f.close()
    f = open(coordinateFile, 'ab')


    
        
    c = coord.SkyCoord(ra= 266.4051 * u.deg,
                   dec= -28.93175 * u.deg,
                   distance= 8122 * u.pc)

    c = c.cartesian

    x = np.single(c.x.value)
    y = np.single(c.y.value)
    z = np.single(c.z.value)

    temper = 1

    global maxX
    global maxY
    global maxZ

    if abs(x) > maxX:
        maxX = abs(x)

    if abs(y) > maxY:
        maxY = abs(y)

    if abs(z) > maxZ:
        maxZ = abs(z)

    appendToFile(f, x, y, z, temper, 8122, 1, np.longlong(1))
    global queriedStars
    queriedStars = queriedStars + 1

    handleResults(f, results2)
        
    f.close()

    filename = 'gdr2_testResults.txt'
    results2.write(filename, format='ascii',overwrite=True)

    print("")
    Gaia.remove_jobs([job2.jobid])

    print("Starting to parse values")
    parseValues(maxValue)
    print("")
    print("Parsed Values")


if __name__ == '__main__':
    main()