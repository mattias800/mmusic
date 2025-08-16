import React, { useState, useEffect } from 'react';
import { useMutation } from 'urql';
import { graphql } from '@/gql';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Alert } from '@/components/ui/Alert';
import { CheckCircle, AlertCircle, Loader2 } from 'lucide-react';

const UPDATE_LISTENBRAINZ_CREDENTIALS = graphql(`
  mutation UpdateUserListenBrainzCredentials($input: UpdateUserListenBrainzCredentialsInput!) {
    updateUserListenBrainzCredentials(input: $input) {
      ... on UpdateUserListenBrainzCredentialsSuccess {
        user {
          id
          username
          listenBrainzUserId
          hasListenBrainzToken
        }
      }
      ... on UpdateUserListenBrainzCredentialsError {
        message
      }
    }
  }
`);

interface ListenBrainzSettingsFormProps {
  userId: string;
  currentListenBrainzUserId?: string | null;
  hasListenBrainzToken: boolean;
  onUpdate?: () => void;
}

export function ListenBrainzSettingsForm({
  userId,
  currentListenBrainzUserId,
  hasListenBrainzToken,
  onUpdate
}: ListenBrainzSettingsFormProps) {
  const [listenBrainzUserId, setListenBrainzUserId] = useState(currentListenBrainzUserId || '');
  const [tokenInput, setTokenInput] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  const [updateCredentialsResult, updateCredentials] = useMutation(UPDATE_LISTENBRAINZ_CREDENTIALS);

  // Reset form when props change
  useEffect(() => {
    setListenBrainzUserId(currentListenBrainzUserId || '');
    setTokenInput('');
    setIsEditing(false);
    setShowSuccess(false);
  }, [currentListenBrainzUserId, hasListenBrainzToken]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const input = {
      userId,
      listenBrainzUserId: listenBrainzUserId || null,
      listenBrainzToken: tokenInput || null
    };

    const result = await updateCredentials({ input });
    
    if (result.data?.updateUserListenBrainzCredentials.__typename === 'UpdateUserListenBrainzCredentialsSuccess') {
      setShowSuccess(true);
      setTokenInput(''); // Clear token input after successful save
      setIsEditing(false);
      onUpdate?.();
      
      // Hide success message after 3 seconds
      setTimeout(() => setShowSuccess(false), 3000);
    }
  };

  const handleEdit = () => {
    setIsEditing(true);
    setShowSuccess(false);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setTokenInput('');
    setListenBrainzUserId(currentListenBrainzUserId || '');
    setShowSuccess(false);
  };

  const isLoading = updateCredentialsResult.fetching;
  const error = updateCredentialsResult.error;

  return (
    <div className="w-full">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-white mb-2">Manage Credentials</h3>
        <p className="text-gray-300 text-sm">
          Update your ListenBrainz account credentials to enable automatic listening history reporting.
        </p>
      </div>
      
      {showSuccess && (
        <Alert variant="success" className="mb-6">
          <div className="flex items-center gap-2">
            <CheckCircle className="h-4 w-4" />
            ListenBrainz credentials updated successfully!
          </div>
        </Alert>
      )}

      {error && (
        <Alert variant="error" className="mb-6">
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            {error.message}
          </div>
        </Alert>
      )}

      {!isEditing ? (
        <div className="space-y-4">
          <div className="p-4 bg-white/5 rounded-lg border border-white/10">
            <Label className="text-sm font-medium text-gray-300">ListenBrainz Username</Label>
            <div className="mt-2 text-white font-medium">
              {currentListenBrainzUserId || 'Not configured'}
            </div>
          </div>
          
          <div className="p-4 bg-white/5 rounded-lg border border-white/10">
            <Label className="text-sm font-medium text-gray-300">API Token</Label>
            <div className="mt-2 text-white font-medium">
              {hasListenBrainzToken ? '••••••••••••••••' : 'Not configured'}
            </div>
          </div>
          
          <Button onClick={handleEdit} className="w-full bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 text-white border-0 shadow-lg">
            Edit Settings
          </Button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="p-4 bg-white/5 rounded-lg border border-white/10">
            <Label htmlFor="listenBrainzUserId" className="text-gray-200 text-sm font-medium">ListenBrainz Username</Label>
            <Input
              id="listenBrainzUserId"
              type="text"
              value={listenBrainzUserId}
              onChange={(e) => setListenBrainzUserId(e.target.value)}
              placeholder="Enter your ListenBrainz username"
              disabled={isLoading}
              className="mt-2 bg-white/10 border-white/20 text-white placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400"
            />
          </div>
          
          <div className="p-4 bg-white/5 rounded-lg border border-white/10">
            <Label htmlFor="tokenInput" className="text-gray-200 text-sm font-medium">API Token</Label>
            <Input
              id="tokenInput"
              type="password"
              value={tokenInput}
              onChange={(e) => setTokenInput(e.target.value)}
              placeholder={hasListenBrainzToken ? "Enter new token (or leave empty to keep current)" : "Enter your ListenBrainz API token"}
              disabled={isLoading}
              className="mt-2 bg-white/10 border-white/20 text-white placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400"
            />
            <p className="mt-3 text-xs text-gray-400">
              {hasListenBrainzToken 
                ? "Leave empty to keep your current token" 
                : "You can find your API token in your ListenBrainz account settings"
              }
            </p>
          </div>
          
          <div className="flex gap-3">
            <Button 
              type="submit" 
              disabled={isLoading} 
              className="flex-1 bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600 text-white border-0 shadow-lg"
            >
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Saving...
                </>
              ) : (
                'Save Changes'
              )}
            </Button>
            <Button 
              type="button" 
              variant="outline" 
              onClick={handleCancel}
              disabled={isLoading}
              className="flex-1 border-white/20 text-white hover:bg-white/10 hover:border-white/30"
            >
              Cancel
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}
