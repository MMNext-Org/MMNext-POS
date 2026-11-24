-- Storage Bucket Setup Migration
-- This migration sets up organization-specific storage buckets with RLS policies

-- Enable RLS on storage tables
ALTER TABLE storage.buckets ENABLE ROW LEVEL SECURITY;

-- Create organization-specific bucket policies
-- Each organization can only access its own bucket
CREATE POLICY organization_bucket_access_policy
    ON storage.buckets
    FOR ALL
    TO authenticated
    USING (bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid)
    WITH CHECK (bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid);

-- Additional policy for bucket creation (only organization owners can create buckets for their organization)
CREATE POLICY bucket_creation_policy
    ON storage.buckets
    FOR INSERT
    TO authenticated
    USING (bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid)
    WITH CHECK (
        bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid
        AND auth.role() = 'owner'
    );

-- Policy for bucket modification (only organization owners can modify buckets for their organization)
CREATE POLICY bucket_modification_policy
    ON storage.buckets
    FOR UPDATE
    TO authenticated
    USING (bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid)
    WITH CHECK (
        bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid
        AND auth.role() = 'owner'
    );

-- Policy for bucket deletion (only organization owners can delete buckets for their organization)
CREATE POLICY bucket_deletion_policy
    ON storage.buckets
    FOR DELETE
    TO authenticated
    USING (bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid)
    WITH CHECK (
        bucket_id = 'bucket_org_' || current_setting('app.schemas.s3.organization_id')::uuid
        AND auth.role() = 'owner'
    );

-- Note: In Supabase, bucket configuration is typically managed through the dashboard
-- This file documents the intended bucket policy structure