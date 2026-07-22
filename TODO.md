1. Get Delete query parameters, if property is not a reference type nor a nullable, check against default and assign only if not equals to default
2. Get Delete query parameters, if property have json property name, use that for query parameter name
3. Remove general Process method with interface to fully qualified names