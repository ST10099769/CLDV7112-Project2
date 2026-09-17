# B. Using services for improving the customer experience

## Azure Event Hubs

**Description of service**
Azure Event Hubs is a big-data streaming platform and event ingestion
service capable of receiving and processing millions of events per second.
It's built for high-throughput, real-time telemetry and clickstream-style
data, not individual transactional messages.

**Mechanism**
Producers (e.g. ABC Retail's web and mobile apps) publish events into a
partitioned event stream. Multiple independent consumers (analytics
pipelines, fraud-detection services, recommendation engines) can read the
same stream in parallel, each tracking its own position (offset) in the
stream, using the Event Hubs Capture feature to also archive raw events to
Blob Storage for later batch analysis.

**How it adds value to end users**
For ABC Retail, Event Hubs would replace the current on-premises
event-processing setup, ingesting every customer interaction — page views,
searches, cart additions, clicks — as a continuous stream. This enables
near real-time personalisation (e.g. "customers who viewed this also
liked...") and lets the company react to demand spikes during peak seasons
like Christmas without the delays the current infrastructure suffers from.

## Azure Service Bus

**Description of service**
Azure Service Bus is an enterprise message broker designed for reliable,
ordered, transactional messaging between decoupled application components
— the cloud-native replacement for ABC Retail's legacy middleware queuing
system.

**Mechanism**
Messages are sent to a queue or topic/subscription. Service Bus guarantees
each message is delivered and processed (with dead-lettering for failed
messages, duplicate detection, and optional strict FIFO ordering via
sessions). Unlike Event Hubs' streaming model, each message is typically
consumed and removed by a single consumer, which suits discrete business
transactions like order processing.

**How it adds value to end users**
For ABC Retail, Service Bus would handle order-processing workflows: an
order placed by a customer becomes a message that flows reliably through
payment validation, inventory reservation, and shipping notification steps
— even if one downstream system is temporarily unavailable. This directly
addresses the "message delays and processing errors" and "missed sales
opportunities" problems mentioned in the scenario, giving customers
consistent, timely order confirmations instead of the current queuing
system's unreliability.

## Summary distinction (useful for your answer)

| | Event Hubs | Service Bus |
|---|---|---|
| Best for | High-volume event streaming/telemetry | Reliable transactional messaging |
| Consumers | Many, read independently | Typically one consumer per message |
| Ordering guarantee | Per-partition | Strict FIFO available via sessions |
| ABC Retail use case | Customer behaviour/clickstream analytics | Order processing pipeline |

*(Rewrite this in your own words before submitting — this is a structural
draft to build your answer from, not something to copy verbatim.)*
