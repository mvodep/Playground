# Check consistency with contstraints

Just some small example how to ensure consistency over severall rows.

Useful links:
* https://www.cybertec-postgresql.com/en/triggers-to-enforce-constraints/

Requirements:
* Mileage per vehicle must always increase by data
* Only one mileage per day (ensures clear order)

Points of interest:
* Function will run in the outer transaction. So the good thing is, that the ROW-LOCK will be also true after the function exit. Check with:

`SELECT * FROM pgrowlocks('vehicle_mileages') JOIN playground.vehicle_mileages ON vehicle_mileages.ctid = locked_row`