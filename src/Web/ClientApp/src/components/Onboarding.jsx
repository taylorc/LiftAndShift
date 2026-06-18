import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { OnboardingClient, SaveUserOnboardingCommand } from '../web-api-client';
import { useAuth } from './api-authorization/AuthContext';
import { Profile } from './shared/profile';


export function Onboarding() {
  return (
    <Profile heading="Set Up Your Profile" subTitle="Enter your starting weights and preferences to begin your program." />
     
  );
}
