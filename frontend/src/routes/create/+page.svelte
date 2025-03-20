<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  
  let appName = '';
  let stack = '.net'; // default stack or could be empty
  let loading = false;
  let successMessage = '';
  let errorMessage = '';
  let registering = false;
  let registerSuccessMessage = '';
  let registerErrorMessage = '';
  let savingSecrets = false;
  let secretsSuccessMessage = '';
  let secretsErrorMessage = '';
  
  // Store registration details for secrets
  let tenantId = '';
  let subscriptionId = '';
  let clientId = '';
  
  // Track progress state for UI indicators
  let step1Complete = false;
  let step2Complete = false;
  let step3Complete = false;

  async function submitForm() {
    loading = true;
    successMessage = '';
    errorMessage = '';

    try {
      const response = await fetch('http://localhost:5264/api/apps', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ AppName: appName, Stack: stack })
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to create application');
      }

      const result = await response.json();
      successMessage = `Application created! Repo: ${result.repositoryUrl}`;
      step1Complete = true; // Mark step 1 as complete
    } catch (err) {
      console.error(err);
      if(err instanceof Error) {
        errorMessage = err.message;
      } else {
      errorMessage = 'Something went wrong.';
      }
    } finally {
      loading = false;
    }
  }

  async function registerApp() {
    if (!appName.trim()) {
      registerErrorMessage = 'Please enter an application name first';
      return;
    }

    registering = true;
    registerSuccessMessage = '';
    registerErrorMessage = '';

    try {
      const response = await fetch(`http://localhost:5264/api/apps/${appName}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ AppName: appName, Stack: stack })
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to register application');
      }

      const result = await response.json();
      // Store the returned values for use with secrets
      tenantId = result.tenantId;
      subscriptionId = result.subscriptionId;
      clientId = result.clientId;
      
      registerSuccessMessage = `App registration created! Client ID: ${result.clientId}`;
      step2Complete = true; // Mark step 2 as complete
    } catch (err) {
      console.error(err);
      if(err instanceof Error) {
        registerErrorMessage = err.message;
      } else {
        registerErrorMessage = 'Something went wrong during registration.';
      }
    } finally {
      registering = false;
    }
  }
  
  // New function to add repository secrets
  async function addSecrets() {
    if (!appName.trim()) {
      secretsErrorMessage = 'Please enter an application name first';
      return;
    }
    
    if (!clientId || !tenantId || !subscriptionId) {
      secretsErrorMessage = 'Please create an app registration first';
      return;
    }

    savingSecrets = true;
    secretsSuccessMessage = '';
    secretsErrorMessage = '';

    try {
      const response = await fetch(`http://localhost:5264/api/apps/${appName}/secrets`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ 
          AppName: appName, 
          TenantId: tenantId, 
          SubscriptionId: subscriptionId, 
          ClientId: clientId 
        })
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to add secrets to repository');
      }

      const result = await response.json();
      secretsSuccessMessage = `Repository secrets added successfully: ${result.status}`;
      step3Complete = true; // Mark step 3 as complete
    } catch (err) {
      console.error(err);
      if(err instanceof Error) {
        secretsErrorMessage = err.message;
      } else {
        secretsErrorMessage = 'Something went wrong while adding secrets.';
      }
    } finally {
      savingSecrets = false;
    }
  }
  
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
  
  .form-group {
    margin-bottom: 15px;
  }
  
  label {
    display: block;
    margin-bottom: 5px;
    font-weight: bold;
  }
  
  input, select {
    width: 100%;
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
    box-sizing: border-box;
    font-size: 16px;
  }
  
  .button-group {
    margin-top: 20px;
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
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
  
  .secondary-btn {
    background-color: #2196F3;
    color: white;
  }
  
  .tertiary-btn {
    background-color: #9C27B0;
    color: white;
  }
  
  .back-btn {
    background-color: #888;
    color: white;
    margin-bottom: 20px;
  }
  
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
  
  /* Progress indicator styles */
  .progress-steps {
    display: flex;
    justify-content: space-between;
    margin: 30px 0;
    position: relative;
  }
  
  .progress-steps::before {
    content: '';
    position: absolute;
    top: 15px;
    left: 0;
    right: 0;
    height: 2px;
    background: #e0e0e0;
    z-index: 1;
  }
  
  .step {
    position: relative;
    z-index: 2;
    background: white;
    width: 30px;
    height: 30px;
    border-radius: 50%;
    border: 2px solid #e0e0e0;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
  }
  
  .step-label {
    position: absolute;
    top: 35px;
    left: 50%;
    transform: translateX(-50%);
    white-space: nowrap;
    font-size: 12px;
    color: #666;
  }
  
  .active {
    border-color: #2196F3;
    background-color: #e3f2fd;
  }
  
  .complete {
    border-color: #4CAF50;
    background-color: #4CAF50;
    color: white;
  }
</style>

<div class="container">
  <button class="back-btn" on:click={goBack}>← Back to Applications</button>
  
  <h1>Create a New Application</h1>
  
  <!-- Progress steps -->
  <div class="progress-steps">
    <div class="step {step1Complete ? 'complete' : loading ? 'active' : ''}" title="Create GitHub Repository">
      1
      <span class="step-label">Create Repository</span>
    </div>
    <div class="step {step2Complete ? 'complete' : (step1Complete && registering) ? 'active' : ''}" title="Register in Entra ID">
      2
      <span class="step-label">Register App</span>
    </div>
    <div class="step {step3Complete ? 'complete' : (step2Complete && savingSecrets) ? 'active' : ''}" title="Add GitHub Secrets">
      3
      <span class="step-label">Add Secrets</span>
    </div>
  </div>

  <form on:submit|preventDefault={submitForm}>
    <div class="form-group">
      <label for="appName">App Name:</label>
      <input
        type="text"
        id="appName"
        bind:value={appName}
        placeholder="Enter your application name"
        required
      />
    </div>

    <div class="form-group">
      <label for="stack">Stack:</label>
      <select id="stack" bind:value={stack}>
        <option value=".net">.NET</option>
        <option value="node">Node.js</option>
        <option value="go">Go</option>
      </select>
    </div>

    <div class="button-group">
      <button type="submit" class="primary-btn" disabled={loading}>
        {#if loading}
          Creating...
        {:else}
          1. Create Repository
        {/if}
      </button>
      
      <button 
        type="button" 
        class="secondary-btn"
        on:click={registerApp} 
        disabled={registering || !step1Complete}
      >
        {#if registering}
          Registering...
        {:else}
          2. Register Application
        {/if}
      </button>
      
      <button 
        type="button"
        class="tertiary-btn" 
        on:click={addSecrets} 
        disabled={savingSecrets || !clientId || !step2Complete}
      >
        {#if savingSecrets}
          Adding Secrets...
        {:else}
          3. Add Repository Secrets
        {/if}
      </button>
    </div>
  </form>

  <!-- Messages section -->
  <div class="messages">
    {#if successMessage}
      <div class="message success">{successMessage}</div>
    {/if}

    {#if errorMessage}
      <div class="message error">{errorMessage}</div>
    {/if}

    {#if registerSuccessMessage}
      <div class="message success">{registerSuccessMessage}</div>
    {/if}

    {#if registerErrorMessage}
      <div class="message error">{registerErrorMessage}</div>
    {/if}

    {#if secretsSuccessMessage}
      <div class="message success">{secretsSuccessMessage}</div>
    {/if}

    {#if secretsErrorMessage}
      <div class="message error">{secretsErrorMessage}</div>
    {/if}
  </div>
</div>
