<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  
  // Define the Application interface to match the API response structure
  interface Application {
    rowKey: string;
    repositoryUrl?: string;
    isRegistered: boolean;
    secretsAdded: boolean;
    azureAppClientId?: string;
    tenantId?: string;
    subscriptionId?: string;
  }
  
  let applications: Application[] = [];
  let loadingApps = true;
  let errorMessage = '';
  
  onMount(async () => {
    try {
      const response = await fetch('http://localhost:5264/api/apps');
      
      if (!response.ok) {
        throw new Error('Failed to fetch applications');
      }
      
      applications = await response.json();
    } catch (err: any) {
      console.error(err);
      errorMessage = err.message || 'Failed to load applications';
    } finally {
      loadingApps = false;
    }
  });
  
  function navigateToCreate() {
    goto('/create');
  }
  
  function navigateToAppDetail(appName: string) {
    goto(`/app/${appName}`);
  }
</script>

<style>
  .container {
    max-width: 1000px;
    margin: 0 auto;
    padding: 20px;
    font-family: Arial, sans-serif;
  }
  
  h1 {
    color: #333;
    text-align: center;
    margin-bottom: 30px;
  }
  
  .message {
    padding: 10px;
    border-radius: 4px;
    margin-top: 15px;
  }
  
  .error {
    background-color: #ffeaea;
    color: #dc3545;
    border: 1px solid #dc3545;
  }
  
  button {
    padding: 10px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: bold;
    transition: background-color 0.3s;
    background-color: #4CAF50;
    color: white;
    margin-bottom: 20px;
  }
  
  .app-list {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
  }
  
  .app-list th, .app-list td {
    border: 1px solid #ddd;
    padding: 12px;
    text-align: left;
  }
  
  .app-list th {
    background-color: #f2f2f2;
    font-weight: bold;
  }
  
  .app-list tr:nth-child(even) {
    background-color: #f9f9f9;
  }
  
  .app-list tr:hover {
    background-color: #f1f1f1;
  }
  
  .status-indicator {
    display: inline-block;
    width: 12px;
    height: 12px;
    border-radius: 50%;
    margin-right: 5px;
  }
  
  .status-complete {
    background-color: #4CAF50;
  }
  
  .status-pending {
    background-color: #FFC107;
  }
  
  .status-label {
    display: flex;
    align-items: center;
  }
  
  .repo-link {
    color: #2196F3;
    text-decoration: none;
  }
  
  .repo-link:hover {
    text-decoration: underline;
  }
  
  .no-apps {
    text-align: center;
    margin-top: 30px;
    color: #666;
  }
  
  .loading {
    text-align: center;
    margin-top: 30px;
    color: #666;
  }
  
  .edit-btn {
    padding: 6px 12px;
    background-color: #2196F3;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }
  
  .edit-btn:hover {
    background-color: #0b7dda;
  }
</style>

<div class="container">
  <h1>Application Dashboard</h1>
  
  <button on:click={navigateToCreate}>Create New Application</button>
  
  {#if loadingApps}
    <div class="loading">Loading applications...</div>
  {:else if errorMessage}
    <div class="message error">{errorMessage}</div>
  {:else if applications.length === 0}
    <div class="no-apps">No applications found. Create your first one!</div>
  {:else}
    <table class="app-list">
      <thead>
        <tr>
          <th>Application Name</th>
          <th>Repository</th>
          <th>Registration Status</th>
          <th>Secrets Status</th>
          <th>Client ID</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {#each applications as app}
          <tr>
            <td>{app.rowKey}</td>
            <td>
              {#if app.repositoryUrl}
                <a href={app.repositoryUrl} class="repo-link" target="_blank">
                  {app.repositoryUrl.split('/').pop()}
                </a>
              {:else}
                N/A
              {/if}
            </td>
            <td>
              <span class="status-label">
                <span class="status-indicator {app.isRegistered ? 'status-complete' : 'status-pending'}"></span>
                {app.isRegistered ? 'Registered' : 'Not Registered'}
              </span>
            </td>
            <td>
              <span class="status-label">
                <span class="status-indicator {app.secretsAdded ? 'status-complete' : 'status-pending'}"></span>
                {app.secretsAdded ? 'Added' : 'Not Added'}
              </span>
            </td>
            <td>{app.azureAppClientId || 'N/A'}</td>
            <td>
              <button class="edit-btn" on:click={() => navigateToAppDetail(app.rowKey)}>Edit</button>
            </td>
          </tr>
        {/each}
      </tbody>
    </table>
  {/if}
</div>
