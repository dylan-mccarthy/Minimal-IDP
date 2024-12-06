<script>
  import { onMount } from 'svelte';
  
  let appName = '';
  let stack = '.net'; // default stack or could be empty
  let loading = false;
  let successMessage = '';
  let errorMessage = '';

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
</script>

<h1>Create a New Application</h1>

<form on:submit|preventDefault={submitForm}>
  <div>
    <label for="appName">App Name:</label>
    <input
      type="text"
      id="appName"
      bind:value={appName}
      placeholder="Enter your application name"
      required
    />
  </div>

  <div>
    <label for="stack">Stack:</label>
    <!-- This could be a dropdown if you want to allow multiple stacks -->
    <select id="stack" bind:value={stack}>
      <option value=".net">.NET</option>
      <option value="node">Node.js</option>
      <option value="go">Go</option>
      <!-- Add other stacks as needed -->
    </select>
  </div>

  <button type="submit" disabled={loading}>
    {#if loading}
      Creating...
    {:else}
      Create Application
    {/if}
  </button>
</form>

{#if successMessage}
  <p style="color:green;">{successMessage}</p>
{/if}

{#if errorMessage}
  <p style="color:red;">{errorMessage}</p>
{/if}
