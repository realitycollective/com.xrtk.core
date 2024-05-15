# Reality Toolkit - Core

![com.realitytoolkit.core](https://github.com/realitycollective/realitycollective.logo/blob/main/RealityToolkit/RepoBanners/com.realitytoolkit.core.png?raw=true)

The core module of the [Reality Toolkit](https://www.realitytoolkit.io/) contains base implementations used across toolkit modules and is mandatory in any project using the toolkit.

[![openupm](https://img.shields.io/npm/v/com.realitytoolkit.core?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.realitytoolkit.core/) [![Discord](https://img.shields.io/discord/597064584980987924.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/hF7TtRCFmB)
[![Publish main branch and increment version](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/main-publish.yml/badge.svg)](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/main-publish.yml)
[![Publish development branch on Merge](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/development-publish.yml/badge.svg)](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/development-publish.yml)
[![Build and test UPM packages for platforms, all branches except main](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/development-buildandtestupmrelease.yml/badge.svg)](https://github.com/realitycollective/com.realitytoolkit.core/actions/workflows/development-buildandtestupmrelease.yml)

## Installation

Make sure to always use the same source for all toolkit modules. Avoid using different installation sources within the same project. We provide the following ways to install Reality Toolkit modules:

### Method 1: OpenUPM CLI

This method requires the [OpenUPM CLI](https://openupm.com/#get-started-with-cli-optional) to be installed on your computer.

```text
    openupm add com.realitytoolkit.core
```

### Method 2: OpenUPM Scoped Registry

If you do not wish to use the [OpenUPM CLI](https://openupm.com/#get-started-with-cli-optional) you can also manually add the OpenUPM registry to your project and browse all available toolkit packages.

1. Open the [Project Settings](https://docs.unity3d.com/Manual/comp-ManagerGroup.html) window.
   
2. Select the **Package Manager** settings category to the left.
   
3. Add a new scoped registry
   1. Name: **OpenUPM**
   2. URL: **https://package.openupm.com**
   3. Scopes: **com.realitycollective** and **com.realitytoolkit**
   4. Press **Save** 

![Add Scoped Registry](https://github.com/realitycollective/realitycollective.logo/blob/main/RealityToolkit/ReadmeAssets/add-scoped-registry.png?raw=true)

4. Open the [Package Manager](https://docs.unity3d.com/Manual/Packages.html) window.
   
5. In the top left packages filter dropdown select **My Registries**.
   
6. You'll now see all published toolkit packages listed for you to install.

![Add Scoped Registry](https://github.com/realitycollective/realitycollective.logo/blob/main/RealityToolkit/ReadmeAssets/package-manager-registry.png?raw=true)

### Method 3: Using Package Manager for git users

1. Open the [Package Manager](https://docs.unity3d.com/Manual/Packages.html) through this menu: **Window -> Package Manager**.

2. Inside the [Package Manager](https://docs.unity3d.com/Manual/Packages.html), click on the **+** button on the top left and select **Add package from git URL...**.

3. Input the following URL: https://github.com/realitycollective/com.realitytoolkit.core.git and click **Add**.

### Method 4: Unity Asset Store

We are working on making the Reality Toolkit available via the Unity asset store and will share news, once available.

## Getting Started

Check the ["Getting Started"](https://www.realitytoolkit.io/) documentation for the Reality Toolkit and to learn more about this module.
