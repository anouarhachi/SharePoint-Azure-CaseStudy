import * as React from 'react';
import styles from './CaseStudySender.module.scss';
import { ICaseStudySenderProps } from './ICaseStudySenderProps';
import { SPHttpClient, SPHttpClientResponse } from '@microsoft/sp-http';


export interface ICaseStudyItem {
  Id: number;
  Title: string;
}

export default function CaseStudySender(props: ICaseStudySenderProps) {
  const [items, setItems] = React.useState<ICaseStudyItem[]>([]);
  const [status, setStatus] = React.useState<string>("Initializing...");

  
  React.useEffect(() => {
    setStatus("Loading items from SharePoint...");
    
    props.spHttpClient.get(
      `${props.siteUrl}/_api/web/lists/getbytitle('CaseStudyList')/items?$select=Id,Title`,
      SPHttpClient.configurations.v1
    )
    .then((response: SPHttpClientResponse) => {
        if (response.ok) {
            return response.json();
        } else {
            throw new Error(`SharePoint Error: ${response.statusText}`);
        }
    })
    .then((data) => {
      if (data && data.value) {
          setItems(data.value);
          setStatus(`Loaded ${data.value.length} items from SharePoint.`);
      }
    })
    .catch((error) => {
      setStatus(`Error loading list: ${error}`);
    });
  }, [props.siteUrl, props.spHttpClient]); 

  
  const sendToAzure = async (item: ICaseStudyItem) => {
    setStatus(`Sending Item ${item.Id} to Azure...`);
    
    
    const azureUrl = "http://localhost:7071/api/ReceiveItem";
    
    const bodyData = JSON.stringify({
      id: item.Id,
      name: item.Title,
      source: "SPFx WebPart"
    });

    try {
      
      const response = await fetch(azureUrl, {
        method: 'POST',
        body: bodyData,
        headers: { 'Content-Type': 'application/json' }
      });

      if (response.ok) {
        const result = await response.text();
        setStatus(`Success! Azure responded: "${result}"`);
      } else {
        setStatus(`Azure Error: ${response.statusText}`);
      }
    } catch (e) {
      setStatus(`Failed to reach Azure (Is it running?): ${e}`);
    }
  };

  return (
    <section className={styles.caseStudySender}>
      <div style={{ padding: '20px', backgroundColor: 'white', border: '1px solid #ddd' }}>
        <h2>SharePoint to Azure Connector</h2>
        
        <div style={{ padding: '10px', background: '#eef', marginBottom: '15px', borderRadius: '4px' }}>
          <strong>Status:</strong> {status}
        </div>
        
        {items.length === 0 && <p>No items found in 'CaseStudyList'.</p>}

        <ul style={{ listStyle: 'none', padding: 0 }}>
          {items.map((item) => (
            <li key={item.Id} style={{ 
                borderBottom: '1px solid #eee', 
                padding: '10px', 
                display: 'flex', 
                justifyContent: 'space-between',
                alignItems: 'center' 
            }}>
              <span style={{ fontSize: '16px', fontWeight: 'bold' }}>{item.Title} (ID: {item.Id})</span>
              <button 
                onClick={() => sendToAzure(item)}
                style={{
                    backgroundColor: '#0078d4',
                    color: 'white',
                    border: 'none',
                    padding: '8px 16px',
                    cursor: 'pointer',
                    borderRadius: '4px'
                }}>
                Send to Azure
              </button>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}