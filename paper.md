---
title: "Neobem: A domain specific language for generating EnergyPlus and DOE-2 input files"
tags:
  - Building energy simulation
  - Domain specific language

authors:
  - name: Mitchell T. Paulus
    orcid: 0000-0001-6662-8165
    affiliation: "1 2"
    corresponding: true
  - name: Kushan B. Abeyawardhane
    orcid: 0000-0002-1324-5416
    affiliation: "1 2"

affiliations:
  - name: Command Commissioning, LLC
    index: 1
  - name: Texas A&M University, Energy Systems Laboratory, College Station, TX, USA
    index: 2

bibliography: joss.bib
date: 27 August 2022
---

# Summary

Building energy simulation plays a key role in the design and operation of buildings.
An important simulation program is EnergyPlus, developed by the United States Department of Energy [@Crawley2001].
EnergyPlus is used extensively in both academic and industrial settings.

The input for EnergyPlus simulations are simple text files,
with data formatted in "Objects" separated by semi-colons (`;`) and fields separated by commas (`,`). For example:

```
Building,
  PSI HOUSE DORM AND OFFICES, !- Name
  36.87000,                   !- North Axis {deg}
  Suburbs,                    !- Terrain
  0.04,                       !- Loads Convergence Tolerance Value
  0.4000000,                  !- Temperature Convergence Tolerance Value {deltaC}
  FullInteriorAndExterior,    !- Solar Distribution
  40,                         !- Maximum Number of Warmup Days
  6;                          !- Minimum Number of Warmup Days
```

Most users will use GUI software such as OpenStudio to create simple building energy models.
However, as with any data entry, a point comes quickly when researchers and developers will want to remove duplication and parameterize models as concisely as possible.

Neobem is a programming language and corresponding compiler designed to meet this need with the following considerations in mind:

1. Neobem files are a superset of the original input files.
3. Expressive syntax designed for the domain.
2. Simplified installation and limited dependencies.

# Statement of Need

Neobem is focused on parameterizing the initial model creation in an expressive and succinct manner.

EP-Macro [@EpMacro2021]

OpenStudio [@Guglielmetti2011]

Eplusr [@Jia2021]

Eppy [@eppy]

# Key Syntax Features

Two pieces of syntax

Integration with the building component library.

Inline data tables


# References
