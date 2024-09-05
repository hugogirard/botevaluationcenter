# Table of Contents

- [Introduction](#introduction)
- [Architecture](#architecture)
- [Configure Knowledge Bases](#configure-knowledge-bases)
  - [Leveraging Azure App Configuration and the External Store Pattern](#leveraging-azure-app-configuration-and-the-external-store-pattern)

# Introduction

This project leverages Azure Language Service to create a sophisticated chatbot that interacts with multiple knowledge bases using the Retrieval-Augmented Generation (RAG) pattern. It is designed to handle user queries by searching through pre-configured knowledge bases, which are maintained by domain experts.

If an answer is found, it can be further refined by an optional Large Language Model (LLM) using advanced prompt engineering techniques. Users can provide feedback on the accuracy of the answers, enabling continuous improvement.

In cases where the answer is not found in the knowledge bases, the system will search external sources and use the LLM to generate a specific response. If no reliable answer is available, the user has the option to request assistance from a human agent.

# Architecture

![arch](/img/architecture.png)

# Configure Knowledge Bases

### Leveraging Azure App Configuration and the External Store Pattern

In this project, we utilize Azure App Configuration and the external store pattern to manage and configure all the knowledge bases efficiently. Here's how it works:

1. **Azure App Configuration**:

   - **Centralized Management**: Azure App Configuration provides a centralized repository to manage application settings and feature flags. This allows us to store configuration data for all knowledge bases in one place.
   - **Dynamic Configuration**: By using Azure App Configuration, we can dynamically update the settings without redeploying the application. This is particularly useful for adding, removing, or updating knowledge bases on the fly.
   - **Security and Access Control**: Azure App Configuration ensures that sensitive configuration data is securely stored and access-controlled, providing a secure way to manage knowledge base configurations.

2. **External Store Pattern**:
   - **Decoupling Configuration from Code**: The external store pattern helps in decoupling the configuration data from the application code. This means that the knowledge base configurations are stored externally in Azure App Configuration rather than being hardcoded in the application.
   - **Scalability**: By using an external store, we can easily scale the application and manage configurations across multiple instances without inconsistencies.
   - **Flexibility**: The external store pattern provides the flexibility to integrate with various data sources and services, making it easier to manage complex configurations.

### Configuration saved in Azure App Configuration

The knowledge base configurations are stored in Azure App Configuration as JSON settings. Each knowledge base is defined with the following properties:

```json
{
  "KnowledgeBase": {
    "KnowledgeConfiguration": [
      {
        "displayName": "Quebec Fun Fact Knowledge Base",
        "name": "quebecfunfact",
        "appRoles": "Admin,QuebecAdmin"
      },
      {
        "displayName": "Canada QnA Knowledge Base",
        "name": "canadaqna",
        "appRoles": "Admin,CanadaAdmin"
      }
    ]
  }
}
```

| Property      | Description                                                                                       |
| ------------- | ------------------------------------------------------------------------------------------------- |
| `displayName` | The display name of the knowledge base.                                                           |
| `name`        | The unique identifier for the knowledge base (need to be the name inside Azure Language Service). |
| `appRoles`    | The application roles that have access to this knowledge base.                                    |

# Azure Language Service

The knowledge base is stored in Azure Language Service, where it is created using the Azure Language Service and assigned a unique knowledge base ID for querying purposes.

This sample includes two datasets located in the KBs folder. The datasets are in the form of XLSX files and are uploaded to Azure Language Service to create the knowledge base.

# Deploy the solution

In progress ...

### Configure Knowledge Bases

Go to your resource group where all
