<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  
  // Extract application name from the URL parameter
  const appName = $page.params.appName;
  
  // Define Application interface matching our backend model
  interface Application {
    rowKey: string;           // Application name (primary key)
    repositoryUrl?: string;   // GitHub repository URL
    isRegistered: boolean;    // Whether the application is registered in Entra ID
    secretsAdded: boolean;    // Whether GitHub secrets have been added
    azureAppClientId?: string; // Azure Application (Client) ID 
    tenantId?: string;        // Azure Tenant ID
    subscriptionId?: string;  // Azure Subscription ID
  }
  
  // Application state variables
  let application: Application | null = null;
  let loading = true;
  let errorMessage = '';
  
  // Action state variables for UI feedback
  let registering = false;
  let registerSuccess = '';
  let registerError = '';
  
  let addingSecrets = false;
  let secretsSuccess = '';
  let secretsError = '';
  
  let deleting = false;
  let deleteSuccess = '';
  let deleteError = '';
  
  // Confirmation dialog state
  let showDeleteConfirm = false;
  
  // Load application details when component mounts
  onMount(async () => {
    await loadApplicationDetails();
  });
  
  /**
   * Fetches application details from the API
   * Populates the application state variable with data
   */
  async function loadApplicationDetails() {
    try {
      loading = true;
      errorMessage = '';
      
      const response = await fetch(`http://localhost:5264/api/apps/${appName}`);
      
      if (!response.ok) {
        throw new Error('Failed to fetch application details');
      }
      
      application = await response.json();
    } catch (err: any) {
      console.error(err);
      errorMessage = err.message || 'Failed to load application details';
    } finally {
      loading = false;
    }
  }
  
  /**
   * Recreates application registration in Entra ID
   * This will create a new application registration even if one exists
   * After successful creation, the application state is updated
   */
  async function recreateAppRegistration() {
    try {
      registering = true;
      registerSuccess = '';
      registerError = '';
      
      const response = await fetch(`http://localhost:5264/api/apps/${appName}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ AppName: appName })
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to register application');
      }
      
      const result = await response.json();
      registerSuccess = `App registration recreated! Client ID: ${result.clientId}`;
      
      // Refresh application details to show updated state
      await loadApplicationDetails();
    } catch (err: any) {
      console.error(err);
      registerError = err.message || 'Failed to recreate application registration';
    } finally {
      registering = false;
    }
  }
  
  /**
   * Updates GitHub repository secrets with Azure credentials
   * This sends the current application Azure details to be set as GitHub secrets
   * After successful update, the application state is refreshed
   */
  async function addRepositorySecrets() {
    if (!application) return;
    
    try {
      addingSecrets = true;
      secretsSuccess = '';
      secretsError = '';
      
      const response = await fetch(`http://localhost:5264/api/apps/${appName}/secrets`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          AppName: appName,
          TenantId: application.tenantId,
          SubscriptionId: application.subscriptionId,
          ClientId: application.azureAppClientId
        })
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to add secrets to repository');
      }
      
      const result = await response.json();
      secretsSuccess = `Repository secrets updated successfully: ${result.status}`;
      
      // Refresh application details to show updated state
      await loadApplicationDetails();
    } catch (err: any) {
      console.error(err);
      secretsError = err.message || 'Failed to update repository secrets';
    } finally {
      addingSecrets = false;
    }
  }
  
  /**
   * Deletes the application from our system
   * This removes the application from our database but preserves the GitHub repository
   * Note: This does not delete the Entra ID application, which would require additional API support
   */
  async function deleteApplication() {
    try {
      deleting = true;
      deleteSuccess = '';
      deleteError = '';
      
      // Currently we don't have a delete endpoint, so the API would need to be updated
      const response = await fetch(`http://localhost:5264/api/apps/${appName}`, {
        method: 'DELETE'
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to delete application');
      }
      
      deleteSuccess = 'Application deleted successfully';
      
      // Navigate back to the main page after successful deletion
      setTimeout(() => {
        goto('/');
      }, 2000);
    } catch (err: any) {
      console.error(err);
      deleteError = err.message || 'Failed to delete application';
    } finally {
      deleting = false;
      showDeleteConfirm = false;
    }
  }
  
  // Navigate back to the main applications page
  function goBack() {
    goto('/');
  }
</script>

<style>
  .container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    font-family: Arial, sans-serif;
  }
  
  h1 {
    color: #333;
    text-align: center;
    margin-bottom: 30px;
  }
  
  h2 {
    color: #444;
    margin-top: 30px;
    margin-bottom: 15px;
    border-bottom: 1px solid #eee;
    padding-bottom: 10px;
  }
  
  .back-btn {
    background-color: #888;
    color: white;
    padding: 8px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    margin-bottom: 20px;
  }
  
  .app-details {
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 20px;
    margin-bottom: 30px;
  }
  
  /* Row layout for detail items */
  .detail-row {
    display: flex;
    margin-bottom: 10px;
    align-items: flex-start;
  }
  
  .detail-label {
    font-weight: bold;
    width: 150px;
    min-width: 150px;
  }
  
  .detail-value {
    flex: 1;
    word-break: break-word;
  }
  
  .repo-link {
    color: #2196F3;
    text-decoration: none;
  }
  
  .repo-link:hover {
    text-decoration: underline;
  }
  
  /* Status indicators for visual feedback */
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
  
  /* Button group layout */
  .action-buttons {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
    margin-top: 20px;
  }
  
  button {
    padding: 10px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: bold;
    transition: background-color 0.3s;
  }
  
  button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
  
  .primary-btn {
    background-color: #4CAF50;
    color: white;
  }
  
  .primary-btn:hover {
    background-color: #3e8e41;
  }
  
  .secondary-btn {
    background-color: #2196F3;
    color: white;
  }
  
  .secondary-btn:hover {
    background-color: #0b7dda;
  }
  
  .danger-btn {
    background-color: #f44336;
    color: white;
  }
  
  .danger-btn:hover {
    background-color: #d32f2f;
  }
  
  /* Message styles for feedback */
  .message {
    padding: 10px;
    border-radius: 4px;
    margin-top: 15px;
  }
  
  .success {
    background-color: #e6ffed;
    color: #28a745;
    border: 1px solid #28a745;
  }
  
  .error {
    background-color: #ffeaea;
    color: #dc3545;
    border: 1px solid #dc3545;
  }
  
  .loading {
    text-align: center;
    margin-top: 50px;
    color: #666;
  }
  
  /* Confirmation dialog styles */
  .confirm-dialog {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 100;
  }
  
  .confirm-content {
    background-color: white;
    padding: 20px;
    border-radius: 4px;
    width: 400px;
    max-width: 90%;
  }
  
  .confirm-title {
    margin-top: 0;
    color: #dc3545;
  }
  
  .confirm-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    margin-top: 20px;
  }
</style>

<div class="container">
  <button class="back-btn" on:click={goBack}>← Back to Applications</button>
  
  <h1>Application Details: {appName}</h1>
  
  {#if loading}
    <div class="loading">Loading application details...</div>
  {:else if errorMessage}
    <div class="message error">{errorMessage}</div>
  {:else if application}
    <!-- Application details section -->
    <div class="app-details">
      <div class="detail-row">
        <div class="detail-label">Application Name:</div>
        <div class="detail-value">{application.rowKey}</div>
      </div>
      
      <div class="detail-row">
        <div class="detail-label">Repository URL:</div>
        <div class="detail-value">
          {#if application.repositoryUrl}
            <a href={application.repositoryUrl} class="repo-link" target="_blank">{application.repositoryUrl}</a>
          {:else}
            Not available
          {/if}
        </div>
      </div>
      
      <div class="detail-row">
        <div class="detail-label">Registration Status:</div>
        <div class="detail-value">
          <span class="status-label">
            <span class="status-indicator {application.isRegistered ? 'status-complete' : 'status-pending'}"></span>
            {application.isRegistered ? 'Registered' : 'Not Registered'}
          </span>
        </div>
      </div>
      
      <div class="detail-row">
        <div class="detail-label">Secrets Status:</div>
        <div class="detail-value">
          <span class="status-label">
            <span class="status-indicator {application.secretsAdded ? 'status-complete' : 'status-pending'}"></span>
            {application.secretsAdded ? 'Added' : 'Not Added'}
          </span>
        </div>
      </div>
      
      {#if application.azureAppClientId}
        <div class="detail-row">
          <div class="detail-label">Client ID:</div>
          <div class="detail-value">{application.azureAppClientId}</div>
        </div>
      {/if}
      
      {#if application.tenantId}
        <div class="detail-row">
          <div class="detail-label">Tenant ID:</div>
          <div class="detail-value">{application.tenantId}</div>
        </div>
      {/if}
      
      {#if application.subscriptionId}
        <div class="detail-row">
          <div class="detail-label">Subscription ID:</div>
          <div class="detail-value">{application.subscriptionId}</div>
        </div>
      {/if}
    </div>
    
    <!-- Application management actions -->
    <h2>Manage Application</h2>
    <div class="action-buttons">
      <button 
        class="secondary-btn" 
        on:click={recreateAppRegistration} 
        disabled={registering}
      >
        {#if registering}
          Recreating Registration...
        {:else}
          Recreate App Registration
        {/if}
      </button>
      
      <button 
        class="primary-btn" 
        on:click={addRepositorySecrets} 
        disabled={addingSecrets || !application.isRegistered}
      >
        {#if addingSecrets}
          Updating Secrets...
        {:else}
          Update Repository Secrets
        {/if}
      </button>
      
      <button 
        class="danger-btn"
        on:click={() => showDeleteConfirm = true}
      >
        Delete Application
      </button>
    </div>
    
    <!-- Operation result messages -->
    {#if registerSuccess}
      <div class="message success">{registerSuccess}</div>
    {/if}
    
    {#if registerError}
      <div class="message error">{registerError}</div>
    {/if}
    
    {#if secretsSuccess}
      <div class="message success">{secretsSuccess}</div>
    {/if}
    
    {#if secretsError}
      <div class="message error">{secretsError}</div>
    {/if}
    
    {#if deleteSuccess}
      <div class="message success">{deleteSuccess}</div>
    {/if}
    
    {#if deleteError}
      <div class="message error">{deleteError}</div>
    {/if}
  {:else}
    <div class="message error">Application not found</div>
  {/if}
  
  <!-- Delete confirmation dialog -->
  {#if showDeleteConfirm}
    <div class="confirm-dialog">
      <div class="confirm-content">
        <h3 class="confirm-title">Delete Application?</h3>
        <p>
          This will remove the application "{appName}" from the system and delete its Entra ID registration if it exists.
          <strong>The GitHub repository will not be deleted.</strong>
        </p>
        <p>This action cannot be undone. Are you sure you want to continue?</p>
        
        <div class="confirm-buttons">
          <button 
            class="secondary-btn" 
            on:click={() => showDeleteConfirm = false}
            disabled={deleting}
          >
            Cancel
          </button>
          <button 
            class="danger-btn" 
            on:click={deleteApplication}
            disabled={deleting}
          >
            {deleting ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  {/if}
</div>
