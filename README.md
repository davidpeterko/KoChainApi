# KoChainApi

KoChain is a .NET Core–based blockchain learning and experimentation platform.
It is designed with a layered architecture to separate concerns across API, Core domain logic, and Infrastructure.

The project leverages [NBitcoin](https://github.com/MetacoSA/NBitcoin), a comprehensive .NET library for Bitcoin, to handle blockchain-related operations.  

---

## 📂 Project Structure

- **KoChain.Api**  
  The Web API layer (ASP.NET Core).  
  - Exposes endpoints for interacting with KoChain.  
  - Handles HTTP requests and responses.  
  - Depends on `KoChain.Core` for domain logic.

- **KoChain.Core**  
  The core domain and business logic.  
  - Contains entities, value objects, and services.  
  - Implements the blockchain concepts and rules.  
  - Independent of any external frameworks.

- **KoChain.Infrastructure**  
  The persistence and external services layer.  
  - Handles data storage (e.g., database, file system).  
  - Provides implementations for repositories, RPC clients, and other integrations.  
  - Depends on `KoChain.Core`.

---

## 🔧 RPC Client (Local Testing)

KoChain requires access to a Bitcoin node for certain operations.  
For local development and testing:  

TBD

---

## Documentation & Learning Goals

KoChain is primarily an educational project for:

Understanding blockchain principles in a .NET Core environment.

Practicing clean architecture and modular design.

Learning integration with Bitcoin RPC via NBitcoin.

---

## Acknowledgements & License

NBitcoin
 — Copyright (c) 2011–2024 Nicolas Dorier & Contributors
Licensed under the MIT License

This project uses NBitcoin as a dependency for Bitcoin and blockchain-related features.





