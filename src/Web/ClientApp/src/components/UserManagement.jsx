import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { OnboardingClient, SaveUserOnboardingCommand } from '../web-api-client';
import { useAuth } from './api-authorization/AuthContext';
import { Profile } from './shared/profile';


export function UserManagement() {
  return (
    <Profile heading="Update Your Profile" subTitle="MOdify your starting weights and preferences to restart your program." />
     
  );
}
