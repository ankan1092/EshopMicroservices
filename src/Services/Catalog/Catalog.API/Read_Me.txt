

**Project Folder Structure of Catalog Microservice**

1. **Models**:
    - Contains all the domain models.
    
2. **Features**:
    - Organized by vertical slices, each feature represents a specific business functionality.
    - Contains subfolders for commands, queries, handlers, and related classes.
    - Example: The **Products** folder.

3. **Data**:
    - Manages all database interactions.
    - Contains the context objects and repositories for accessing and manipulating data.
    
4. **Abstractions**:
    - Defines the interfaces and base classes that are implemented by other parts of the project.

5. **CQRS Pattern**:
    - The project employs Command Query Responsibility Segregation (CQRS) to separate the read and write operations.
    - Utilizes the **Mediator** library to implement the CQRS pattern effectively.

6. **Vertical Slice Architecture**:
    - Implements the vertical slice architecture to ensure each feature is self-contained and independent.

7. **Database**:
    - Uses PostgreSQL with the Marten library to provide transactional database capabilities for .NET applications.
