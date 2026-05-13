using System;
using System.Collections.Generic;

namespace MechanicShop.Client.Models;


public class CustomerModel
{
    public Guid CustomerId {get;set;}
    public string Name {get;set;} = string.Empty;
    public string PhoneNumber {get;set;} = string.Empty;
    public string Email {get;set;} = string.Empty;

    public List<VehicleModel> Vehicles {get;set;} = [];
}