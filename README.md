# BookingApi

### Regular registration via Identity and JwtToken, the default role is User
![](docs/register.gif)

### The login also overwrites the JwtToken when logging in
![](docs/login.gif)

### Only the admin can create, modify, delete Housing.
![](docs/createHousing.gif)

### If the Housing that had a booking is deleted, then the User who booked it will lose this booking
![](docs/HousingDelete.gif)

### When booking, the user specifies the HousingId they want to book, and if everything is successful, the isBooked status changes to true, and the user who booked it is added (the token is taken from the header)
![](docs/BookHousing.gif)

### When unbooking, the isBooked status changes to false and the user who booked is removed from Housing
![](docs/UnBookHousing.gif)