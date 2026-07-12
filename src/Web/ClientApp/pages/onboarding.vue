<script setup lang="ts">
import { MiddlewareList, MiddlewareName } from '~/lib/middlewareList';

const list = new MiddlewareList();
definePageMeta({ middleware: list.getStringValueForEmum(MiddlewareName.Auth)})
</script>

<template>
  <SharedProfile
    heading="Set Up Your Profile"
    sub-title="Enter your starting weights and preferences to begin your program."
  />
</template>
