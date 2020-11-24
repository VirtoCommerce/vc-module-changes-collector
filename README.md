# VirtoCommerce.ChangesCollectorModule
[![License](https://img.shields.io/badge/license-VC%20OSL-blue.svg)](https://virtocommerce.com/open-source-license)

Experimental module to implement the scope/module-based changes collector.

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
