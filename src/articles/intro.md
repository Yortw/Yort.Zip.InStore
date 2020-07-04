# Introduction

This library is a thin/light weight OOP wrapper around the [Zip API](https://docs-nz.zip.co/instore-api). It provides a clean OOP style interface and saves you having to do generate your own models, make HTTP calls, serialise/deserialise requests and responses and so on. It doesn't add any retry logic, persistent stores or reliability handling. It is still up to the application developer to provide a [robust implementation](articles/productionrequirements.html).

This library only implements the [Zip in-store/POS API](https://docs-nz.zip.co/instore-api) and does not implement e-commerce flows.
