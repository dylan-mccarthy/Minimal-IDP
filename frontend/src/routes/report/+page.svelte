<script lang="ts">
  import { onMount } from 'svelte';
  import { Pie } from 'svelte-chartjs';
  import { Chart, ArcElement, Tooltip, Legend } from 'chart.js';

  Chart.register(ArcElement, Tooltip, Legend);

  interface Application {
    appName: string;
    age: number;
  }

  let applications: Application[] = [];
  let loading = true;
  let errorMessage = '';

  onMount(async () => {
    try {
      const response = await fetch('http://localhost:5264/api/apps/report');

      if (!response.ok) {
        throw new Error('Failed to fetch report data');
      }

      applications = await response.json();
    } catch (err: any) {
      console.error(err);
      errorMessage = err.message || 'Failed to load report data';
    } finally {
      loading = false;
    }
  });

  const pieData = {
    labels: applications.map(app => app.appName),
    datasets: [
      {
        data: applications.map(app => app.age),
        backgroundColor: [
          '#FF6384',
          '#36A2EB',
          '#FFCE56',
          '#4BC0C0',
          '#9966FF',
          '#FF9F40'
        ]
      }
    ]
  };

  const pieOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top'
      },
      tooltip: {
        callbacks: {
          label: function (context: any) {
            const label = context.label || '';
            const value = context.raw || 0;
            return `${label}: ${value} days old`;
          }
        }
      }
    }
  };
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

  .loading {
    text-align: center;
    margin-top: 30px;
    color: #666;
  }
</style>

<div class="container">
  <h1>Applications Report</h1>

  {#if loading}
    <div class="loading">Loading report data...</div>
  {:else if errorMessage}
    <div class="message error">{errorMessage}</div>
  {:else}
    <Pie {pieData} {pieOptions} />
  {/if}
</div>
