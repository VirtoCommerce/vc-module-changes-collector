# VirtoCommerce.ChangesCollectorModule
[![License](https://img.shields.io/badge/license-VC%20OSL-blue.svg)](https://virtocommerce.com/open-source-license)

Experimental module to implement the scope/module-based changes collector.

There is a problem with the standard endpoint to discover platform object changes: the scope of changes is all the system.
This module allows to flexibly define scopes by grouping entity storage types. If one of the entity instances had changed, the scope counts as changed.
The module adds 2 endpoints:
1. To know the last changed DateTime. GET:'~/api/changes-collector/last-modified-date'. 
2. To know the last changed DateTimefor all endpoints. GET:'~/api/changes-collector/last-modified-date-all-scopes'. 

Scopes definitions can be read from 'VirtoCommerce.ChangesCollectorModule.Scopes' setting or from a file named 'changes-collector-scopes.json'.
Scopes JSON format (example):
```
{
  "Stores": [
"VirtoCommerce.StoreModule.Data.Model.StoreEntity",
"VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity",
"VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity"
  ],
  "Orders": [
"VirtoCommerce.OrdersModule.Data.Model.CustomerOrderEntity",
"VirtoCommerce.OrdersModule.Data.Model.ShipmentEntity",
"VirtoCommerce.OrdersModule.Data.Model.PaymentInEntity"
  ]
}
```
