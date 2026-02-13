# Ingestion Pipeline Design

## Approach

1. Read documents from storage
2. Extract text from binaries (e.g., PDF, DOCX)
3. Scrub PHI/PII from the extracted text
4. Use an LLM to infer document-level metadata (fields shared by all chunks)
5. Chunk the scrubbed text
6. Create embeddings for each chunk
7. Use an LLM to infer chunk-level metadata (topic, subcategory, scope, etc.)
8. Combine:
     - embeddings
     - chunk text
     - document-level metadata
     - chunk-level metadata
9. Insert records into the vector database


## Technologies Proposed
### Encrypted storage
* Just in case we do have anything sensitive
* VeraCrypt to store knowledge base - virtual (mountable) encrypted disk 
### Python
* Strong ecosystem of libraries
#### PHI/PII scrubbing
* Microsoft Presidio
#### PDF extraction
* `pypdf`, `pdfplumber`, `unstructured` 
### Embeddings
* Embeddings are numbers that capture the meaning of something - in this case a chunk of text - so the vector database can compare them and quickly find the things that "feel" most similar.

* Create embeddings using the ollama command within a python script, then feed that into a vector database
```python
def embed(text):
    result = subprocess.run(
        ["ollama", "embeddings", "-m", "nomic-embed-text"],
        input=text.encode("utf-8"),
        capture_output=True
    )
    return json.loads(result.stdout)["embedding"]
```

### Vector database - Qdrant
* Can be run via docker, has free cloud tier (but ...), and if wildly successful, could be moved to Qdrant Cloud

#### Schema
```json
{
  "vectors": {
    "size": 768,
    "distance": "Cosine"
  },
  "hnsw_config": {
    "m": 16,
    "ef_construct": 100
  },
  "optimizers_config": {
    "default_segment_number": 2
  },
  "quantization_config": null,
  "on_disk_payload": true,
  "on_disk_vectors": false,
  "shard_number": 1,
  "replication_factor": 1,
  "write_consistency_factor": 1,
  "schema": {
    "insurer": "keyword",
    "plan_type": "keyword",
    "employer_group": "keyword",
    "state": "keyword",
    "topic": "keyword",
    "subcategory": "keyword",
    "scope": "keyword",
    "source_type": "keyword",
    "document_id": "keyword",
    "chunk_id": "keyword",
    "chunk_index": "integer",
    "text": "text"
    }

}
```

#### Field meanings
* `insurer`: The specific insurer this chunk applies to (e.g., Aetna). Empty for general knowledge.
* `plan_type`: The plan category within that insurer (e.g., PPO, HMO, EPO). Empty if not plan‑specific.
* `employer_group`: The employer or group plan identifier when applicability is limited to a specific company.
* `state`: The U.S. state whose regulations or rules this chunk depends on.
* `topic`: The high‑level subject area (e.g., prior authorization, appeals, step therapy).
* `subcategory`: A more precise subtopic within the main topic (e.g., peer review, medical necessity).
* `scope`: How broadly the chunk applies (e.g., general, insurer, insurer_plan_type, insurer_plan_type_employer, uncertain, derived).
* `source_type`: The type of document the chunk came from (e.g., policy, denial letter, regulation, call transcript).
* `document_id`: A stable identifier linking the chunk back to its original document.
* `chunk_id`: A unique identifier for this specific chunk.
* `chunk_index`: The chunk’s position within the document, used to reconstruct order.
* `text`: The actual text content of the chunk used for embedding and retrieval.